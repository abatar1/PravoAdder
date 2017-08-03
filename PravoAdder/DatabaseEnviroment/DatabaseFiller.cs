using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
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

        public void Authentication(string login, string password)
        {
            HttpAuthenticator.Authentication(login, password);
            DatabaseGetter.Authentication(login, password);
        }

        private static async Task<HttpResponseMessage> SendAddRequest(object content, string uri, HttpMethod method)
        {
            var request = HttpHelper.CreateRequest(content, $"api/{uri}", method, HttpAuthenticator.UserCookie);

            var response = await HttpAuthenticator.Client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return response;
        }

        public void AddProjectGroup(string projectGroupName, string folderName, string description = null)
        {
            var content = new
            {
                Name = projectGroupName,
                ProjectFolder = DatabaseGetter.GetProjectFolder(folderName),
                Description = description
            };

            var response = SendAddRequest(content, "ProjectGroups", HttpMethod.Put).Result;
        }

        public string AddProject(string projectName, string folderName, string projectTypeName, string responsibleName,
            string projectGroupName)
        {
            var content = new
            {
                ProjectFolder = DatabaseGetter.GetProjectFolder(folderName),
                ProjectType = DatabaseGetter.GetProjectType(projectTypeName),
                Responsible = DatabaseGetter.GetResponsible(responsibleName),
                ProjectGroup = DatabaseGetter.GetProjectGroup(projectGroupName),
                Name = projectName
            };

            var response = SendAddRequest(content, "projects/CreateProject", HttpMethod.Post).Result;

            var responseContent = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(responseContent)["Result"]["Id"].ToString();
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
