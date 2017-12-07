using System.Net.Http;
using System.Threading.Tasks;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ProjectCustomValuesApi
	{
		public VisualBlockModel Create(HttpAuthenticator httpAuthenticator, VisualBlock content)
		{
			return ApiHelper.GetItem<VisualBlockModel>(httpAuthenticator, "ProjectCustomValues/Create", HttpMethod.Post, content);
		}

		public async Task<bool> Update(HttpAuthenticator httpAuthenticator, VisualBlock content)
		{
			return await ApiHelper.TrySendAsync(httpAuthenticator, "ProjectCustomValues/Update", HttpMethod.Put, content);
		}

		public VisualBlockWrapper GetAllVisualBlocks(HttpAuthenticator httpAuthenticator, string projectId)
		{
			var parameter = ApiHelper.CreateParameters(("ProjectId", projectId));
			return ApiHelper.GetItem<VisualBlockWrapper>(httpAuthenticator, "ProjectCustomValues/GetAllVisualBlocks", HttpMethod.Get, parameter);
		}

		public VisualBlockWrapper Delete(HttpAuthenticator httpAuthenticator, string projectVisualBlockId)
		{
			var parameter = ApiHelper.CreateParameters(("ProjectVisualBlockId", projectVisualBlockId));
			return ApiHelper.GetItem<VisualBlockWrapper>(httpAuthenticator, "ProjectCustomValues/Delete", HttpMethod.Delete, parameter);
		}
	}
}
