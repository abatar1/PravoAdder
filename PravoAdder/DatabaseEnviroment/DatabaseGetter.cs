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

        private static object GetPage(string name, string uri)
        {
            var content = new
            {
                Name = name,
                PageSize = 20,
                Page = 1
            };

            var request = HttpHelper.CreateRequest(content, $"api/{uri}", HttpMethod.Post, HttpAuthenticator.UserCookie);

            var response = HttpAuthenticator.Client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();

            var jsonMessage = HttpHelper.GetMessageFromResponce(response).Result.Result[0];
            return new
            {
                Id = (string)jsonMessage["Id"],
                Name = (string)jsonMessage["Name"],
            };
        }

        public object GetProjectType(string projectTypeName)
        {
            return GetPage(projectTypeName, "ProjectTypes/GetProjectTypes");
        }

        public object GetProjectGroup(string projectGroupName)
        {
            return GetPage(projectGroupName, "ProjectGroups/PostProjectGroups");
        }

        public object GetResponsible(string resposibleName)
        {
            return GetPage(resposibleName, "CompanyUsersSuggest");
        }

        public object GetProjectFolder(string folderName)
        {
            return GetPage(folderName, "ProjectFolders/GetProjectFoldersForEdit");
        }

        public object GetParticipant(string participantName)
        {
            return GetPage(participantName, "ParticipantsSuggest/GetParticipants");
        }
    }
}
