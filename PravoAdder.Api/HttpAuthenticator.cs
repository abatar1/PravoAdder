using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	[Serializable]
    public class HttpAuthenticator : IDisposable
    {
        public HttpAuthenticator(string baseUri)
        {
            BaseAddress = new Uri(baseUri);
            CookieContainer = new CookieContainer();

            var clientHandler = new HttpClientHandler
            {
                CookieContainer = CookieContainer
            };
            var retryHandler = new RetryHandler(clientHandler, 5);
            Client = new HttpClient(retryHandler)
            {
                BaseAddress = BaseAddress
            };
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public HttpClient Client { get; }
        public Cookie UserCookie { get; private set; }
        private Uri BaseAddress { get; }
        private CookieContainer CookieContainer { get; }

        public void Dispose()
        {
            Client?.Dispose();
        }

        protected void Authentication(string login, string password)
        {
            var authentication = new
            {
                Password = password,
                Login = login
            };

            var response = Client.PostAsJsonAsync("authentication/account/login", authentication).Result;
	        response.EnsureSuccessStatusCode();
            if (!response.IsSuccessStatusCode) throw new AuthenticationException("Failed to send authentication request.");

            UserCookie = CookieContainer.GetCookies(BaseAddress).Cast<Cookie>().FirstOrDefault();
            if (UserCookie == null) throw new AuthenticationException("Cannot create new session");

            var message = ApiHelper.ReadFromResponce(response);
            if (!(bool) message.Succeeded) throw new AuthenticationException("Wrong login or password");
        }
    }
}