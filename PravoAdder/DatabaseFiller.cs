using System;
using System.CodeDom;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PravoAdder
{
    public class DatabaseFiller
    {
        private static readonly HttpClient Client;
        private static readonly CookieContainer CookieContainer;
        private static readonly Uri BaseAddress = new Uri("https://testcarcade.casepro.pro/");
        private static Cookie _userCookie;

        static DatabaseFiller()
        {
            CookieContainer = new CookieContainer();
            var handler = new HttpClientHandler
            {
                CookieContainer = CookieContainer
            };

            Client = new HttpClient(handler)
            {
                BaseAddress = BaseAddress
            };
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void Authentication(string login, string password)
        {
            var authentication = new
            {
                Password = password,
                Login = login
            };
       
            var response = Client.PostAsJsonAsync("authentication/account/login", authentication).Result;  //async
            _userCookie = CookieContainer.GetCookies(BaseAddress).Cast<Cookie>().FirstOrDefault();
            if (_userCookie == null) throw new AuthenticationException("Cannot create new session");

            dynamic message = GetMessageFromResponce(response).Result; //async
            if (!(bool)message.Succeeded) throw new AuthenticationException("Wrong login or password");

            response.EnsureSuccessStatusCode();
        }

        public void AddProject(string projectName, string folderName, string description = null)
        {
            var content = new
            {
                Name = projectName,
                ProjectFolder = GetProjectFolder(folderName),
                Description = description
            };
            var request = CreateRequest(content, "api/ProjectGroups", HttpMethod.Put);

            var response = Client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
        }

        private static async Task<dynamic> GetMessageFromResponce(HttpResponseMessage response)
        {
            var message = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject(message);
        }

        private HttpRequestMessage CreateRequest(object content, string requestUri, HttpMethod method)
        {
            var serializedContent = JsonConvert.SerializeObject(content);
            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = new StringContent(serializedContent, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Cookie", _userCookie.ToString());

            return request;
        }

        private object GetProjectFolder(string folderName)
        {
            var content = new
            {
                Name = folderName,
                PageSize = 20,
                Page = 1
            };
            
            var request = CreateRequest(content, "api/ProjectFolders/GetProjectFoldersForEdit", HttpMethod.Post);      

            var response = Client.SendAsync(request).Result; 
            dynamic message = GetMessageFromResponce(response).Result[0];

            response.EnsureSuccessStatusCode();
            return null;
        }
    }
}
