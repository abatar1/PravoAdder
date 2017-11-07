using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ProjectGroupsApi
	{
		public IList<ProjectGroup> GetMany(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.GetItems<ProjectGroup>(httpAuthenticator, "ProjectGroups/PostProjectGroups", HttpMethod.Post);
		}

		public ProjectGroup Create(HttpAuthenticator httpAuthenticator, object content)
		{
			return ApiHelper.GetItem<ProjectGroup>(httpAuthenticator, "ProjectGroups", HttpMethod.Put, content);
		}

		public void Delete(HttpAuthenticator httpAuthenticator, string projectGroupId)
		{
			var parameters = ApiHelper.CreateParameters(("Id", projectGroupId));
			ApiHelper.SendItem(httpAuthenticator, "ProjectGroups/DeleteProjectGroup", HttpMethod.Delete,
				parameters);
		}

		public void Archive(HttpAuthenticator httpAuthenticator, string projectGroupId)
		{
			var parameters = ApiHelper.CreateParameters(("Id", projectGroupId));
			ApiHelper.SendItem(httpAuthenticator, "ProjectGroups/Archive", HttpMethod.Put,
				parameters);
		}
	}
}
