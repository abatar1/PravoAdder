using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ProjectsApi
	{
		public IList<GroupedProjects> GetGroupedProjects(HttpAuthenticator httpAuthenticator, string folderName, string projectGroupid)
		{
			var projectFolder = ApiRouter.ProjectFolders
				.GetProjectFolders(httpAuthenticator)
				.FirstOrDefault(folder => folder.Name == folderName);

			Dictionary<string, string> parameters = null;
			if (projectFolder == null && folderName != null)
			{
				projectFolder = ApiRouter.ProjectFolders.InsertProjectFolder(folderName, httpAuthenticator);
				parameters = ApiHelper.CreateParameters(("FolderId", projectFolder.Id));
			}
			else if (projectFolder != null && folderName != null)
			{
				parameters = ApiHelper.CreateParameters(("FolderId", projectFolder.Id));
			}
			var content = new Content(parameters);
			return ApiHelper.GetItems<GroupedProjects>(httpAuthenticator, "Projects/GetGroupedProjects",
				HttpMethod.Post, content);
		}

		public Project GetProject(HttpAuthenticator httpAuthenticator, string projectId)
		{
			var parameters = ApiHelper.CreateParameters(("ProjectId", projectId));
			var response = ApiHelper.GetItem(httpAuthenticator, "Projects/GetProject", HttpMethod.Get, parameters);
			return new Project(response);
		}

		public Project CreateProject(HttpAuthenticator httpAuthenticator, dynamic content)
		{
			return ApiHelper.GetItem<Project>(httpAuthenticator, "Projects/CreateProject", HttpMethod.Post, content);
		}

		public void DeleteProject(HttpAuthenticator httpAuthenticator, string projectId)
		{
			var parameters = ApiHelper.CreateParameters(("Id", projectId));
			ApiHelper.GetItem(httpAuthenticator, "Projects/DeleteProject", HttpMethod.Delete, parameters);
		}

		public void ArchiveProject(HttpAuthenticator httpAuthenticator, string projectId)
		{
			var parameters = ApiHelper.CreateParameters(("Id", projectId));
			ApiHelper.GetItem(httpAuthenticator, "projects/Archive", HttpMethod.Put, parameters);
		}

		public void RestoreProject(HttpAuthenticator httpAuthenticator, string projectId)
		{
			var parameters = ApiHelper.CreateParameters(("Id", projectId));
			ApiHelper.GetItem(httpAuthenticator, "Projects/Restore", HttpMethod.Put, parameters);
		}
	}
}
