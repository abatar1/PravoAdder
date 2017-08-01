using System;
using System.CodeDom;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
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
       
            var response = Client.PostAsJsonAsync("authentication/account/login", authentication).Result;
            _userCookie = CookieContainer.GetCookies(BaseAddress).Cast<Cookie>().FirstOrDefault();
            if (_userCookie == null) throw new AuthenticationException("Cannot create new session");

            dynamic message = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
            if (!(bool)message.Succeeded) throw new AuthenticationException("Wrong login or password");

            response.EnsureSuccessStatusCode();
        }

        public void AddProjectAsync(string projectName, object projectFolder = null, string description = null)
        {
            var content = new
            {
                Name = projectName,
                ProjectFolder = projectFolder,
                Description = description
            };

            var message = new HttpRequestMessage(HttpMethod.Put, "api/ProjectGroups")
            {
                Content = new StringContent(content.ToString(), Encoding.UTF8, "application/json")
            };
            message.Headers.Add("Cookie", _userCookie.ToString());

            var response = Client.SendAsync(message).Result;
            //var response = Client.PutAsJsonAsync("api/ProjectGroups", content).Result;
            response.EnsureSuccessStatusCode();
        }
    }
}
