using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ProjectCustomValuesApi
	{
		public VisualBlock Create(HttpAuthenticator httpAuthenticator, dynamic content)
		{
			var response = ApiHelper.GetItem(httpAuthenticator, "ProjectCustomValues/Create", HttpMethod.Post, content);
			return JsonConvert.DeserializeObject<VisualBlock>(response.ToString());
		}


		public async Task<bool> Update(HttpAuthenticator httpAuthenticator, dynamic content)
		{
			return await ApiHelper.TrySendAsync(httpAuthenticator, content, "ProjectCustomValues/Update", HttpMethod.Put);
		}
	}
}
