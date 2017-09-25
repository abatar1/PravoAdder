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
			return ApiHelper.SendWithManyPagesRequest<ProjectFolder>(httpAuthenticator, "ProjectFolders/GetProjectFolders", HttpMethod.Post);
		}

		public ProjectFolder InsertProjectFolder(string name, HttpAuthenticator httpAuthenticator)
		{
			var content = new
			{
				Name = name
			};

			return ApiHelper.SendDatabaseEntityItem<ProjectFolder>(content, "ProjectFolders/InsertProjectFolder", HttpMethod.Post,
				httpAuthenticator);
		}

		public void Delete(HttpAuthenticator httpAuthenticator, string projectFolderId)
		{
			var parameters = new Dictionary<string, string> { ["FolderId"] = projectFolderId };
			ApiHelper.SendItemWithParameters(httpAuthenticator, "ProjectFolders/DeleteProjectFolder", HttpMethod.Delete, parameters);
		}
	}
}
