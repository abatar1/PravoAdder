using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ProjectFoldersApi
	{
		public IList<ProjectFolder> GetProjectFolders(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.GetItems<ProjectFolder>(httpAuthenticator, "ProjectFolders/GetProjectFolders", HttpMethod.Post);
		}

		public ProjectFolder InsertProjectFolder(string name, HttpAuthenticator httpAuthenticator)
		{
			var content = new
			{
				Name = name
			};

			return ApiHelper.GetItem<ProjectFolder>(content, "ProjectFolders/InsertProjectFolder", HttpMethod.Post,
				httpAuthenticator);
		}

		public void Delete(HttpAuthenticator httpAuthenticator, string projectFolderId)
		{
			var parameters = new Dictionary<string, string> { ["FolderId"] = projectFolderId };
			ApiHelper.GetItem(httpAuthenticator, "ProjectFolders/DeleteProjectFolder", HttpMethod.Delete, parameters);
		}
	}
}
