using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ProjectFoldersApi : IGetMany<ProjectFolder>
	{
		public List<ProjectFolder> GetMany(HttpAuthenticator httpAuthenticator, string optional = null)
		{
			return ApiHelper.GetItems<ProjectFolder>(httpAuthenticator, "ProjectFolders/GetProjectFolders", HttpMethod.Post);
		}

		public ProjectFolder Get(HttpAuthenticator authenticator, string parameter)
		{
			throw new System.NotImplementedException();
		}

		public ProjectFolder Insert(string name, HttpAuthenticator httpAuthenticator)
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
			ApiHelper.SendItem(httpAuthenticator, "ProjectFolders/DeleteProjectFolder", HttpMethod.Delete, parameters);
		}
	}
}
