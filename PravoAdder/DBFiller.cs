using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using PravoAdder.Domain;

namespace PravoAdder
{
    public class DBFiller
    {
        private static readonly HttpClient Client = new HttpClient();
        private string _cookie;

        public DBFiller()
        {
            Client.BaseAddress = new Uri("https://testcarcade.casepro.pro/");
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async void Authentication(string login, string password)
        {
            var response = await Client.PostAsJsonAsync("authentication/account/", new Authentication{Name = login, Password = password});
            response.EnsureSuccessStatusCode();
            var cookie = response.Headers;
        }

        public async void AddProjectAsync(string projectName, string projectFolder = null, string description = null)
        {
            var response = await Client.PutAsJsonAsync("api/products/",
                new Project {Name = projectName, Description = description});
            response.EnsureSuccessStatusCode();

        }
    }
}
