using System.Net.Http;
using System.Threading.Tasks;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ProjectCustomValuesApi
	{
		public VisualBlock Create(HttpAuthenticator httpAuthenticator, VisualBlockParticipant content)
		{
			return ApiHelper.GetItem<VisualBlock>(httpAuthenticator, "ProjectCustomValues/Create", HttpMethod.Post, content);
		}

		public async Task<bool> Update(HttpAuthenticator httpAuthenticator, VisualBlockParticipant content)
		{
			return await ApiHelper.TrySendAsync(httpAuthenticator, "ProjectCustomValues/Update", HttpMethod.Put, content);
		}

		public VisualBlockWrapper GetAllVisualBlocks(HttpAuthenticator httpAuthenticator, string projectId)
		{
			var parameter = ApiHelper.CreateParameters(("ProjectId", projectId));
			return ApiHelper.GetItem<VisualBlockWrapper>(httpAuthenticator, "ProjectCustomValues/GetAllVisualBlocks", HttpMethod.Get, parameter);
		}
	}
}
