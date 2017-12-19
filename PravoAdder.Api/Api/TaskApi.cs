using System.Net.Http;
using PravoAdder.Api.Helpers;
using Task = PravoAdder.Api.Domain.Task;

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
