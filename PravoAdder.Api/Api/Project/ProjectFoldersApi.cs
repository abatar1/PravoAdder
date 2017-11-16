using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ProjectFoldersApi : IApi<ProjectFolder>
	{
		public List<ProjectFolder> GetMany(HttpAuthenticator httpAuthenticator, string optional = null)
		{
			return ApiHelper.GetItems<ProjectFolder>(httpAuthenticator, "ProjectFolders/GetProjectFolders", HttpMethod.Post);
		}

		public ProjectFolder Get(HttpAuthenticator authenticator, string parameter)
		{
			throw new System.NotImplementedException();
		}

		public ProjectFolder Create(HttpAuthenticator httpAuthenticator, ProjectFolder folder)
		{
			return ApiHelper.GetItem<ProjectFolder>(httpAuthenticator, "ProjectFolders/InsertProjectFolder", HttpMethod.Post, folder);
		}

		public void Delete(HttpAuthenticator httpAuthenticator, string projectFolderId)
		{
			var parameters = ApiHelper.CreateParameters(("FolderId", projectFolderId));
			ApiHelper.SendItem(httpAuthenticator, "ProjectFolders/DeleteProjectFolder", HttpMethod.Delete, parameters);
		}
	}
}
