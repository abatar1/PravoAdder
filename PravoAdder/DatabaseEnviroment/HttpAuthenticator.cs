using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using PravoAdder.Helper;

namespace PravoAdder.DatabaseEnviroment
{
    public class HttpAuthenticator
    {
        public static HttpClient Client { get; private set; }
        public static Cookie UserCookie { get; private set; }

        public static CookieContainer CookieContainer { get; private set; }
        public static Uri BaseAddress { get; } = new Uri("https://testcarcade.casepro.pro/");

        public HttpAuthenticator()
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
            response.EnsureSuccessStatusCode();

            UserCookie = CookieContainer.GetCookies(BaseAddress).Cast<Cookie>().FirstOrDefault();
            if (UserCookie == null) throw new AuthenticationException("Cannot create new session");

            var message = HttpHelper.GetMessageFromResponce(response).Result;
            if (!(bool)message.Succeeded) throw new AuthenticationException("Wrong login or password");
        }
    }
}