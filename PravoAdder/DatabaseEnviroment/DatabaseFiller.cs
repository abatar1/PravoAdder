using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using PravoAdder.Domain;
using PravoAdder.Domain.Info;

namespace PravoAdder.DatabaseEnviroment
{
    public class DatabaseFiller
    {
        private readonly DatabaseGetter _databaseGetter;
        private readonly HttpAuthenticator _httpAuthenticator;

        public DatabaseFiller(HttpAuthenticator httpAuthenticator)
        {
            _databaseGetter = new DatabaseGetter(httpAuthenticator);
            _httpAuthenticator = httpAuthenticator;
        }

        private async Task<EnviromentMessage> SendAddRequestAsync(object content, string uri, HttpMethod method)
        {
            var request = HttpHelper.CreateRequest(content, $"api/{uri}", method, _httpAuthenticator.UserCookie);

            var response = await _httpAuthenticator.Client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return new EnviromentMessage(await HttpHelper.GetContentIdAsync(response), "Added succefully.");
        }

        public async Task<EnviromentMessage> AddProjectGroupAsync(string projectGroupName, string folderName, string description, bool overwrite = true)
        {
            if (overwrite)
            {
                var projectGroup = _databaseGetter.GetProjectGroup(projectGroupName, 20);
                if (projectGroup != null) return new EnviromentMessage(projectGroup.Id, "Group already exists.");
            }           

            var content = new
            {
                Name = projectGroupName,
                ProjectFolder = _databaseGetter.GetProjectFolder(folderName),
                Description = description
            };

            return await SendAddRequestAsync(content, "ProjectGroups", HttpMethod.Put);
        }

        public async Task<EnviromentMessage> AddProjectAsync(Settings settings, HeaderBlockInfo headerInfo, string projectGroupId, bool overwrite = true)
        {
            if (overwrite)
            {
                var project = _databaseGetter.GetProject(headerInfo.ProjectName, projectGroupId, settings.FolderName, 20);
                if (project != null) return new EnviromentMessage(project.Id, "Project already exists.");
            }

            var content = new
            {
                ProjectFolder = _databaseGetter.GetProjectFolder(settings.FolderName),
                ProjectType = _databaseGetter.GetProjectType(settings.ProjectTypeName),
                Responsible = _databaseGetter.GetResponsible(headerInfo.ResponsibleName),
                ProjectGroup = new
                {
                    Name = headerInfo.ProjectGroupName,
                    Id = projectGroupId
                },
                Name = headerInfo.ProjectName
            };

            return await SendAddRequestAsync(content, "projects/CreateProject", HttpMethod.Post);
        }

        public async Task<EnviromentMessage> AddInformationAsync(string projectId, BlockInfo blockInfo, IDictionary<int, string> excelRow)
        {
            var tmpLines = new List<BlockLineInfo>();
            foreach (var line in blockInfo.Lines)
            {
                var tmpFields = new List<BlockFieldInfo>();
                foreach (var field in line.Fields)
                {
                    var fieldData = excelRow[field.ColumnNumber];
                    if (fieldData == string.Empty) continue;
                    switch (field.Type)
                    {
                        case "Value":
                            field.Value = Convert(fieldData);
                            break;
                        case "Formula":
                            dynamic calculationFormula = _databaseGetter.GetCalculationFormulas(field.SpecialData);
                            field.Value = new
                            {
                                Result = Convert(fieldData),
                                CalculationFormulaId = calculationFormula.Id
                            };
                            break;
                        case "Custom":
                            var correctName = $"{fieldData.First().ToString().ToUpper()}{fieldData.Substring(1)}";
                            field.Value = new
                            {
                                Name = correctName,
                                Id = field.SpecialData,
                                IsCustom = true
                            };
                            break;
                        default:
                            throw new ArgumentException("Unknown type of value.");
                    }
                    tmpFields.Add(field);
                }
                line.Fields = new List<BlockFieldInfo>(tmpFields);
                tmpLines.Add(line);
            }

            var content = new
            {
                VisualBlockId = blockInfo.Id,
                ProjectId = projectId,
                Lines = tmpLines,
                FrontOrder = 0
            };

            return await SendAddRequestAsync(content, "ProjectCustomValues/Create", HttpMethod.Post);
        }

        private static object Convert(string value)
        {
            if (TypeDescriptor.GetConverter(typeof(int)).IsValid(value))
            {
                return int.Parse(value);
            }
            if (TypeDescriptor.GetConverter(typeof(double)).IsValid(value.Replace(',', '.')))
            {
                return double.Parse(value);
            }
            return value;
        }
    }  
}
