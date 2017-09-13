using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Domain;
using PravoAdder.Helpers;

namespace PravoAdder.Api
{
	public class ProjectGroupsApi
	{
		public IList<ProjectGroup> GetProjectGroups(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.SendWithManyPagesRequest<ProjectGroup>(httpAuthenticator, "ProjectGroups/PostProjectGroups", HttpMethod.Post);
		}
	}
}
