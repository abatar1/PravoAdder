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
			return ApiHelper.SendWithManyPagesRequest<ProjectGroup>(httpAuthenticator, "ProjectGroups/PostProjectGroups", HttpMethod.Post);
		}

		public ProjectGroup ProjectGroups(HttpAuthenticator httpAuthenticator, object content)
		{
			return ApiHelper.SendDatabaseEntityItem<ProjectGroup>(content, "ProjectGroups", HttpMethod.Put,
				httpAuthenticator);
		}
	}
}
