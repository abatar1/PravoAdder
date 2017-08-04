using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using PravoAdder.Domain;
using PravoAdder.Domain.Info;

namespace PravoAdder.DatabaseEnviroment
{
    public class DatabaseFiller
    {
        private static readonly HttpAuthenticator HttpAuthenticator;
        private static readonly DatabaseGetter DatabaseGetter;

        static DatabaseFiller()
        {
            HttpAuthenticator = new HttpAuthenticator();
            DatabaseGetter = new DatabaseGetter();
        }

        public bool Authentication(string login, string password)
        {
            try
            {
                HttpAuthenticator.Authentication(login, password);
                DatabaseGetter.Authentication(login, password);
                return true;
            }
            catch (AuthenticationException ex)
            {
                throw new AuthenticationException($"Autentification failed. {ex.Message}", ex);
            }           
        }

        public static async Task<HttpResponseMessage> SendAddRequest(object content, string uri, HttpMethod method)
        {
            var request = HttpHelper.CreateRequest(content, $"api/{uri}", method, HttpAuthenticator.UserCookie);

            var response = await HttpAuthenticator.Client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return response;
        }

        public async Task<EnviromentMessage> AddProjectGroup(string projectGroupName, string folderName, string description)
        {
            var projectGroup = DatabaseGetter.GetProjectGroup(projectGroupName, 20);
            if (projectGroup != null) return new EnviromentMessage(projectGroup.Id, "Group already exists.");

            var content = new
            {
                Name = projectGroupName,
                ProjectFolder = DatabaseGetter.GetProjectFolder(folderName),
                Description = description
            };

            var response = await SendAddRequest(content, "ProjectGroups", HttpMethod.Put);

            return new EnviromentMessage(await HttpHelper.GetContentId(response), "Added succefully.");
        }

        public async Task<EnviromentMessage> AddProject(Settings settings, HeaderBlockInfo headerInfo, string projectGroupId)
        {
            var project = DatabaseGetter.GetProject(headerInfo.ProjectName, projectGroupId, settings.FolderName, 20);
            if (project != null) return new EnviromentMessage(project.Id, "Project already exists.");

            var content = new
            {
                ProjectFolder = DatabaseGetter.GetProjectFolder(settings.FolderName),
                ProjectType = DatabaseGetter.GetProjectType(settings.ProjectTypeName),
                Responsible = DatabaseGetter.GetResponsible(headerInfo.ResponsibleName),
                ProjectGroup = new
                {
                    Name = headerInfo.ProjectGroupName,
                    Id = projectGroupId
                },
                Name = headerInfo.ProjectName
            };

            var response = await SendAddRequest(content, "projects/CreateProject", HttpMethod.Post);

            return new EnviromentMessage(await HttpHelper.GetContentId(response), "Added succefully.");
        }

        public void AddInformation(string projectId, BlockInfo blockInfo, IDictionary<int, string> excelRow)
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
                            dynamic calculationFormula = DatabaseGetter.GetCalculationFormulas(field.SpecialData);
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

            var response = SendAddRequest(content, "ProjectCustomValues/Create", HttpMethod.Post).Result;
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
