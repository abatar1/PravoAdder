using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ProjectGroupsApi : IGetMany<ProjectGroup>
	{
		public List<ProjectGroup> GetMany(HttpAuthenticator httpAuthenticator, string optional = null)
		{
			return ApiHelper.GetItems<ProjectGroup>(httpAuthenticator, "ProjectGroups/PostProjectGroups", HttpMethod.Post);
		}

		public ProjectGroup Get(HttpAuthenticator authenticator, string parameter)
		{
			throw new System.NotImplementedException();
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
