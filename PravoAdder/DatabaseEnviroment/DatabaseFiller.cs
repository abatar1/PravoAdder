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
                var projectGroup = _databaseGetter.GetProjectGroup(projectGroupName);
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
                var project = _databaseGetter.GetProject(headerInfo.ProjectName, projectGroupId, settings.FolderName);
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
            var contentLines = new List<BlockLineInfo>();
            foreach (var line in blockInfo.Lines)
            {
                var contentFields = new List<BlockFieldInfo>();
                foreach (var fieldInfo in line.Fields)
                {
                    var fieldData = excelRow[fieldInfo.ColumnNumber];
                    if (string.IsNullOrEmpty(fieldData)) continue;
	                fieldInfo.Value = CreateFieldValueFromData(fieldInfo, fieldData);
                    contentFields.Add(fieldInfo);
                }
                line.Fields = new List<BlockFieldInfo>(contentFields);
                contentLines.Add(line);
            }

            var contentBlock = new
            {
                VisualBlockId = blockInfo.Id,
                ProjectId = projectId,
                Lines = contentLines,
                FrontOrder = 0
            };

            return await SendAddRequestAsync(contentBlock, "ProjectCustomValues/Create", HttpMethod.Post);
        }

	    private object CreateFieldValueFromData(BlockFieldInfo fieldInfo, string fieldData)
	    {
		    switch (fieldInfo.Type)
		    {
			    case "Value":
				    return FormatFieldData(fieldData);
			    case "Formula":
				    var calculationFormula = _databaseGetter.GetCalculationFormulas(fieldInfo.SpecialData);
				    return new
				    {
					    Result = FormatFieldData(fieldData),
					    CalculationFormulaId = calculationFormula.Id
				    };
			    case "Custom":				    
					var correctName = $"{fieldData.First().ToString().ToUpper()}{fieldData.Substring(1)}";
				    return new
				    {
					    Name = correctName,
					    Id = fieldInfo.SpecialData,
					    IsCustom = true
				    };
			    default:
				    throw new ArgumentException("Unknown type of value.");
		    }
		}

	    private static string FormatIntString(string value)
	    {
		    if (!value.Contains(',')) return value;

		    var newValue = value.Replace(" ", "");
			var splitted = newValue.Split(',');
		    return splitted[1].All(c => c == '0') ? splitted[0] : newValue;
	    }

        private static object FormatFieldData(string value)
        {
	        var correctValue = FormatIntString(value);

			if (TypeDescriptor.GetConverter(typeof(int)).IsValid(correctValue))
            {
                return int.Parse(correctValue);
            }
			if (TypeDescriptor.GetConverter(typeof(double)).IsValid(value.Replace(',', '.')))
            {
                return double.Parse(value);
            }
			return value;
        }
    }  
}
