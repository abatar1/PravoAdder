using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace PravoAdder.DatabaseEnviroment
{
    public class DatabaseGetter
    {
        private static readonly HttpAuthenticator HttpAuthenticator;

        static DatabaseGetter()
        {
            HttpAuthenticator = new HttpAuthenticator();
        }

        public void Authentication(string login, string password)
        {
            HttpAuthenticator.Authentication(login, password);
        }

        private static IEnumerable<dynamic> GetPages(object content, string name, string uri)
        {          
            var request = HttpHelper.CreateRequest(content, $"api/{uri}", HttpMethod.Post, HttpAuthenticator.UserCookie);

            var response = HttpAuthenticator.Client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();

            var jsonMessages = HttpHelper.GetMessageFromResponce(response).Result;
            foreach (var message in jsonMessages.Result)
            {
                yield return message;
            }
        }

        private static object GetSimplePage(string name, string uri)
        {
            var content = new
            {
                Name = name,
                PageSize = 50,
                Page = 1
            };

            var message = GetPages(content, name, uri).FirstOrDefault();

            return new
            {
                Id = (string)message?["Id"],
                Name = (string)message?["Name"],
            };
        }

        public object GetProjectType(string projectTypeName)
        {
            return GetSimplePage(projectTypeName, "ProjectTypes/GetProjectTypes");
        }

        public object GetProjectGroup(string projectGroupName)
        {
            return GetSimplePage(projectGroupName, "ProjectGroups/PostProjectGroups");
        }

        public object GetResponsible(string resposibleName)
        {
            return GetSimplePage(resposibleName.Replace(".", ""), "CompanyUsersSuggest");
        }

        public object GetProjectFolder(string folderName)
        {
            return GetSimplePage(folderName, "ProjectFolders/GetProjectFoldersForEdit");
        }

        public object GetParticipant(string participantName)
        {
            return GetSimplePage(participantName, "ParticipantsSuggest/GetParticipants");
        }

        public object GetCalculationFormulas(string formulaName)
        {
            return GetSimplePage(formulaName, "CalculationFormulasSuggest/GetCalculationFormulas");
        }
    }
}
