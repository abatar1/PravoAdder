﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;


namespace PravoAdder.DatabaseEnviroment
{
    public class HttpAuthenticator
    {
        public HttpClient Client { get; }
        public Cookie UserCookie { get; private set; }       
        private Uri BaseAddress { get; }
        private CookieContainer CookieContainer { get; }

        public HttpAuthenticator(string baseUri)
        {
            BaseAddress = new Uri(baseUri);
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

            var message = HttpHelper.GetMessageFromResponceAsync(response).Result;
            if (!(bool)message.Succeeded) throw new AuthenticationException("Wrong login or password");
        }
    }
}