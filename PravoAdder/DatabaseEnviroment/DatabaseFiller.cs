using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using PravoAdder.Domain;
using PravoAdder.Domain.Info;
using PravoAdder.Helpers;

namespace PravoAdder.DatabaseEnviroment
{
    public class DatabaseFiller
    {
        private readonly DatabaseGetter _databaseGetter;
        private readonly IDictionary<string, ConcurrentBag<DictionaryItem>> _dictionaries;
        private readonly HttpAuthenticator _httpAuthenticator;
        private IList<Participant> _participants;
	    private readonly object _dictionaryContainsKeyLock = new object();

        public DatabaseFiller(HttpAuthenticator httpAuthenticator)
        {
            _databaseGetter = new DatabaseGetter(httpAuthenticator);
            _httpAuthenticator = httpAuthenticator;
            _participants = _databaseGetter.GetParticipants();
			_dictionaries = new ConcurrentDictionary<string, ConcurrentBag<DictionaryItem>>();
        }

        private object CreateFieldValueFromData(BlockFieldInfo fieldInfo, string fieldData)
        {
            if (string.IsNullOrEmpty(fieldData)) return null;

            fieldData = fieldData.Replace("\"", "");
            switch (fieldInfo.Type)
            {
                case "Value":
                    return FormatFieldData(fieldData);
                case "Text":
                    return fieldData;
                case "Formula":
                    var calculationFormula = _databaseGetter.GetCalculationFormulas(fieldInfo.SpecialData);
                    return new
                    {
                        Result = FormatFieldData(fieldData),
                        CalculationFormulaId = calculationFormula.Id
                    };
                case "Dictionary":
                    return GetDictionaryFromData(fieldData, fieldInfo);
                case "Participant":
                    return GetParticipantFromData(fieldData);
                default:
                    throw new ArgumentException("Unknown type of value.");
            }
        }

        #region Add methods

        protected async Task<EnviromentMessage> AddDictionaryItem(string itemName, string sysName)
        {
            var content = new
            {
                SystemName = sysName,
                Name = itemName,
                IsCustom = true
            };

            return await SendAddRequestAsync(content, "Dictionary/SaveDictionaryItem", HttpMethod.Put);
        }

        protected async Task<EnviromentMessage> AddParticipant(string organizationName)
        {
            var content = new
            {
                Organization = organizationName,
                Type = new
                {
                    Id = "92ffb67f-fac0-e611-8b3a-902b343a9588",
                    action = "add",
                    Name = "Организация",
                    NameEn = "company"
                }
            };

            return await SendAddRequestAsync(content, "participants/PutParticipant", HttpMethod.Put);
        }

        protected async Task<EnviromentMessage> AddProjectGroupAsync(Settings settings, HeaderBlockInfo headerInfo)
        {
            if (settings.Overwrite)
            {
                var projectGroup = _databaseGetter.GetProjectGroup(headerInfo.ProjectGroupName);
                if (projectGroup != null)
                    return new EnviromentMessage(projectGroup.Id, "Group already exists.",
                        EnviromentMessageType.Success);
            }

            var projectFolder = _databaseGetter.GetProjectFolder(headerInfo.FolderName);
            if (projectFolder == null)
                return new EnviromentMessage("", "Project folder doesn't exist.", EnviromentMessageType.Error);

            var content = new
            {
                Name = headerInfo.ProjectGroupName,
                ProjectFolder = projectFolder,
                headerInfo.Description
            };

            return await SendAddRequestAsync(content, "ProjectGroups", HttpMethod.Put);
        }

        protected async Task<EnviromentMessage> AddProjectAsync(Settings settings, HeaderBlockInfo headerInfo,
            string projectGroupId)
        {
            if (settings.Overwrite)
            {
                var project = _databaseGetter.GetProject(headerInfo.ProjectName, projectGroupId, headerInfo.FolderName);
                if (project != null)
                    return new EnviromentMessage(project.Id, "Project already exists.", EnviromentMessageType.Success);
            }

            var projectFolder = _databaseGetter.GetProjectFolder(headerInfo.FolderName);
            if (projectFolder == null)
                return new EnviromentMessage("",
                    $"Project folder {headerInfo.FolderName} doesn't exist. Project name: {headerInfo.ProjectName}",
                    EnviromentMessageType.Error);

            var projectType = _databaseGetter.GetProjectType(settings.ProjectTypeName);
            if (projectType == null)
                return new EnviromentMessage("",
                    $"Project type {settings.ProjectTypeName} doesn't exist. Project name: {headerInfo.ProjectName}",
                    EnviromentMessageType.Error);

            var responsible = _databaseGetter.GetResponsible(headerInfo.ResponsibleName);
            if (responsible == null)
                return new EnviromentMessage("",
                    $"Responsible {headerInfo.ResponsibleName} doesn't exist. Project name: {headerInfo.ProjectName}",
                    EnviromentMessageType.Error);

            var content = new
            {
                ProjectFolder = projectFolder,
                ProjectType = projectType,
                Responsible = responsible,
                ProjectGroup = new
                {
                    Name = headerInfo.ProjectGroupName,
                    Id = projectGroupId
                },
                Name = headerInfo.ProjectName
            };

            return await SendAddRequestAsync(content, "projects/CreateProject", HttpMethod.Post);
        }

        protected async Task<EnviromentMessage> AddInformationAsync(string projectId, BlockInfo blockInfo,
            IDictionary<int, string> excelRow)
        {
            var contentLines = new List<BlockLineInfo>();
            if (blockInfo.Lines == null || !blockInfo.Lines.Any())
                return new EnviromentMessage(null, "Block skipped.", EnviromentMessageType.Warning);

            var messageBuilder = new StringBuilder();
            foreach (var line in blockInfo.Lines)
            {
                var contentFields = new List<BlockFieldInfo>();
                foreach (var fieldInfo in line.Fields)
                {
                    if (!excelRow.ContainsKey(fieldInfo.ColumnNumber))
                    {
                        messageBuilder.AppendLine($"Excel row doesn't contain \"{fieldInfo.ColumnNumber}\" key.");
                        continue;
                    }
                    var fieldData = excelRow[fieldInfo.ColumnNumber];

                    if (string.IsNullOrEmpty(fieldData)) continue;

                    try
                    {
                        var value = CreateFieldValueFromData(fieldInfo, fieldData);
                        var newFieldInfo = fieldInfo.CloneWithValue(value);
                        contentFields.Add(newFieldInfo);
                    }
                    catch (Exception e)
                    {
                        messageBuilder.AppendLine(
                            $"Error while reading value from table! Message: {e.Message} Id: {projectId} Data: {fieldData} Type: {fieldInfo.Type}");
                    }
                }
                if (!contentFields.Any()) continue;
                var newLine = line.CloneWithFields(contentFields);
                contentLines.Add(newLine);
            }

            if (contentLines.All(c => !c.Fields.Any()))
                return new EnviromentMessage(null, "Block skipped.", EnviromentMessageType.Warning);

            var contentBlock = new
            {
                VisualBlockId = blockInfo.Id,
                ProjectId = projectId,
                Lines = contentLines,
                FrontOrder = 0
            };

            return await SendAddRequestAsync(contentBlock, "ProjectCustomValues/Create", HttpMethod.Post,
                messageBuilder.ToString());
        }

        private async Task<EnviromentMessage> SendAddRequestAsync(dynamic content, string uri, HttpMethod method,
            string additionalMessage = null)
        {
            var request = HttpHelper.CreateJsonRequest(content, $"api/{uri}", method, _httpAuthenticator.UserCookie);

            var response = await _httpAuthenticator.Client.SendAsync(request);

            if (!string.IsNullOrEmpty(additionalMessage))
                return new EnviromentMessage(await HttpHelper.GetContentIdAsync(response),
                    additionalMessage.Remove(additionalMessage.Length - 2), EnviromentMessageType.Error);

            return !response.IsSuccessStatusCode
                ? new EnviromentMessage(null,
                    $"Failed to send {uri}. Message: {response.ReasonPhrase}. Id: {content.ProjectId}",
                    EnviromentMessageType.Error)
                : new EnviromentMessage(await HttpHelper.GetContentIdAsync(response), "Complete succefully.",
                    EnviromentMessageType.Success);
        }

        #endregion

        #region Additional methods		

        private static object FormatFieldData(string value)
        {
            string correctIntValue;
            if (!value.Contains(','))
            {
                correctIntValue = value;
            }
            else
            {
                var newValue = value.Replace(" ", "");
                var splitted = newValue.Split(',');
                correctIntValue = splitted[1].All(c => c == '0') ? splitted[0] : newValue;
            }

            if (TypeDescriptor.GetConverter(typeof(int)).IsValid(correctIntValue))
                return int.Parse(correctIntValue);

            if (TypeDescriptor.GetConverter(typeof(double)).IsValid(value.Replace(',', '.')))
                return double.Parse(value);
            return value;
        }

        private static string FormatDictionaryItemName(string item)
        {
            return $"{item.First().ToString().ToUpper()}{item.Substring(1)}";
        }

        private Participant GetParticipantFromData(string fieldData)
        {
            var correctFieldData = fieldData.Trim();
            if (_participants.All(p => !p.Name.Equals(correctFieldData)))
            {
                var sender = AddParticipant(fieldData).Result;
                if (sender.Type == EnviromentMessageType.Error) return null;

                _participants = _databaseGetter
                    .GetParticipants();
            }

            var participant = _participants
                .First(p => p.Name == correctFieldData);

            return Participant.TryParse(participant);
        }

        private DictionaryItem GetDictionaryFromData(string fieldData, BlockFieldInfo fieldInfo)
        {
            var dictionaryName = fieldInfo.SpecialData;
            var correctName = FormatDictionaryItemName(fieldData);

	        lock (_dictionaryContainsKeyLock)
	        {
				if (!_dictionaries.ContainsKey(dictionaryName))
				{
					var dictionaryItems = _databaseGetter
						.GetDictionaryItems(fieldInfo.SpecialData)
						.Select(d => new DictionaryItem(FormatDictionaryItemName(d.Name), d.Id))						
						.ToList();
					_dictionaries.Add(dictionaryName, new ConcurrentBag<DictionaryItem>(dictionaryItems));
				}
			}            

            if (_dictionaries[dictionaryName].All(d => !d.Name.Equals(correctName)))
            {
                var sender = AddDictionaryItem(correctName, fieldInfo.SpecialData).Result;
                if (sender.Type == EnviromentMessageType.Error) return null;

                _dictionaries[dictionaryName].Add(new DictionaryItem(correctName, sender.Content));
            }

            return _dictionaries[dictionaryName]
                .First(d => d.Name == correctName);
        }

        #endregion
    }
}