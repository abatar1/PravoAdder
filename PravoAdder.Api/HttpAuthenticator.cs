using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
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

        protected EnviromentMessage Authentication(string login, string password)
        {
            var authentication = new
            {
                Password = password,
                Login = login
            };

            var response = Client.PostAsJsonAsync("authentication/account/login", authentication).Result;
	        response.EnsureSuccessStatusCode();

            if (!response.IsSuccessStatusCode)
                return new EnviromentMessage(null, "Failed authentication request.", EnviromentMessageType.Error);

            UserCookie = CookieContainer.GetCookies(BaseAddress).Cast<Cookie>().FirstOrDefault();
            if (UserCookie == null)
                return new EnviromentMessage(null, "Cannot create new session", EnviromentMessageType.Error);

            var message = ApiHelper.ReadFromResponce(response);
            if (!(bool) message.Succeeded)
                return new EnviromentMessage(null, "Wrong login or password", EnviromentMessageType.Error);

            return new EnviromentMessage(null, "Successful login", EnviromentMessageType.Success);
        }
    }
}