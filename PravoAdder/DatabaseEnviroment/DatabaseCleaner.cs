using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using PravoAdder.Helpers;

namespace PravoAdder.DatabaseEnviroment
{
    public class DatabaseCleaner
    {
        private readonly HttpAuthenticator _httpAuthenticator;

        public DatabaseCleaner(HttpAuthenticator authenticator)
        {
            _httpAuthenticator = authenticator;
        }

        private async Task<bool> TrySendWithoutResponse(IDictionary<string, string> parameters, string uri,
            HttpMethod httpMethod)
        {
            var request = HttpHelper.CreateRequest(parameters, $"api/{uri}", httpMethod, _httpAuthenticator.UserCookie);

            return await TrySendRequest(request);
        }

        private async Task<bool> TrySendRequest(HttpRequestMessage request)
        {
            var response = await _httpAuthenticator.Client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return response.IsSuccessStatusCode;
        }

        protected async Task<EnviromentMessage> DeleteProject(string projectId)
        {
            var parameters = new Dictionary<string, string> {["Id"] = projectId};
            var result = await TrySendWithoutResponse(parameters, "Projects/DeleteProject/{Id}", HttpMethod.Delete);
            return result
                ? new EnviromentMessage("", $"{projectId} deleted succefully", EnviromentMessageType.Success)
                : new EnviromentMessage("", $"Error during deleting {projectId}", EnviromentMessageType.Error);
        }

        protected async Task<EnviromentMessage> DeleteProjectGroup(string projectGroupId)
        {
            var parameters = new Dictionary<string, string> {["Id"] = projectGroupId};
            var result = await TrySendWithoutResponse(parameters, "Projects/DeleteProjectGroup/{Id}",
                HttpMethod.Delete);
            return result
                ? new EnviromentMessage("", $"{projectGroupId} deleted succefully", EnviromentMessageType.Success)
                : new EnviromentMessage("", $"Error during deleting {projectGroupId}", EnviromentMessageType.Error);
        }
    }
}