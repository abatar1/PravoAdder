using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ProjectGroupsApi
	{
		public IList<ProjectGroup> GetProjectGroups(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.GetItems<ProjectGroup>(httpAuthenticator, "ProjectGroups/PostProjectGroups", HttpMethod.Post);
		}

		public ProjectGroup ProjectGroups(HttpAuthenticator httpAuthenticator, object content)
		{
			return ApiHelper.GetItem<ProjectGroup>(httpAuthenticator, "ProjectGroups", HttpMethod.Put, content);
		}

		public void DeleteProjectGroup(HttpAuthenticator httpAuthenticator, string projectGroupId)
		{
			var parameters = new Dictionary<string, string> { ["Id"] = projectGroupId };
			ApiHelper.GetItem(httpAuthenticator, "ProjectGroups/DeleteProjectGroup", HttpMethod.Delete,
				parameters);
		}

		public void ArchiveProjectGroup(HttpAuthenticator httpAuthenticator, string projectGroupId)
		{
			var parameters = new Dictionary<string, string> { ["Id"] = projectGroupId };
			ApiHelper.GetItem(httpAuthenticator, "ProjectGroups/Archive", HttpMethod.Put,
				parameters);
		}
	}
}
