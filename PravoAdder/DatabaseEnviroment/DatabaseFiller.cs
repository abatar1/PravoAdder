using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PravoAdder.Domain;

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

        public void AddGeneralInformation(string projectId, Block generalBlock, IDictionary<int, string> excel)
        {
            var tmpLines = new List<BlockLine>();
            foreach (var line in generalBlock.Lines)
            {
                var tmpFields = new List<BlockField>();
                foreach (var field in line.Fields)
                {
                    field.Value = excel[field.ColumnNumber];
                    tmpFields.Add(field);
                }
                line.Fields = new List<BlockField>(tmpFields);
                tmpLines.Add(line);
            }

            var content = new
            {
                VisualBlockId = generalBlock.Id,
                ProjectId = projectId,
                Lines = tmpLines
            };
            var response = SendAddRequest(content, "ProjectCustomValues/Create", HttpMethod.Post).Result;
        }       
    }
}
