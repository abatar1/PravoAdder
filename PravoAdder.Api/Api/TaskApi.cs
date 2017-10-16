using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
    public class TaskApi
    {
        public void Create(HttpAuthenticator authenticator, Task task)
        {
            var result = ApiHelper.TrySendAsync(authenticator, "tasks", HttpMethod.Put, task).Result;
        }
    }
}
