using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ProjectsApi
	{
		public List<GroupedProjects> GetGroupedProjects(HttpAuthenticator httpAuthenticator, string folderName, string projectGroupid)
		{
			var projectFolder = ApiRouter.ProjectFolders
				.GetProjectFolders(httpAuthenticator)
				.FirstOrDefault(folder => folder.Name == folderName);

			Dictionary<string, string> additionalContent = null;
			if (projectFolder == null && folderName != null)
			{
				projectFolder = ApiRouter.ProjectFolders.InsertProjectFolder(folderName, httpAuthenticator);
				additionalContent = new Dictionary<string, string> { ["FolderId"] = projectFolder.Id };
			}
			else if (projectFolder != null && folderName != null)
			{
				additionalContent = new Dictionary<string, string> { ["FolderId"] = projectFolder.Id };
			}

			return ApiHelper.GetItems<GroupedProjects>(httpAuthenticator, "Projects/GetGroupedProjects",
				HttpMethod.Post, additionalContent)
				.ToList();
		}

		public Project CreateProject(HttpAuthenticator httpAuthenticator, dynamic content)
		{
			return ApiHelper.GetItem<Project>(content, "Projects/CreateProject", HttpMethod.Post,
				httpAuthenticator);
		}

		public void DeleteProject(HttpAuthenticator httpAuthenticator, string projectId)
		{
			var parameters = new Dictionary<string, string> { ["Id"] = projectId };
			ApiHelper.GetItem(httpAuthenticator, "Projects/DeleteProject", HttpMethod.Delete, parameters);
		}

		public void ArchiveProject(HttpAuthenticator httpAuthenticator, string projectId)
		{
			var parameters = new Dictionary<string, string> { ["Id"] = projectId };
			ApiHelper.GetItem(httpAuthenticator, "projects/Archive", HttpMethod.Put, parameters);
		}
	}
}
