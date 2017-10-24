using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ProjectFoldersApi
	{
		public List<ProjectFolder> GetProjectFolders(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.GetItems<ProjectFolder>(httpAuthenticator, "ProjectFolders/GetProjectFolders", HttpMethod.Post);
		}

		public ProjectFolder InsertProjectFolder(string name, HttpAuthenticator httpAuthenticator)
		{
			var content = new
			{
				Name = name
			};

			return ApiHelper.GetItem<ProjectFolder>(httpAuthenticator, "ProjectFolders/InsertProjectFolder", HttpMethod.Post,
				content);
		}

		public void Delete(HttpAuthenticator httpAuthenticator, string projectFolderId)
		{
			var parameters = ApiHelper.CreateParameters(("FolderId", projectFolderId));
			ApiHelper.GetItem(httpAuthenticator, "ProjectFolders/DeleteProjectFolder", HttpMethod.Delete, parameters);
		}
	}
}
