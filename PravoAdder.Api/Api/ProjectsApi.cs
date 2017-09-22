using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ProjectsApi
	{
		public IList<Project> GetProjects(HttpAuthenticator httpAuthenticator, string folderName, string projectGroupid)
		{
			var projectFolder = ApiRouter.ProjectFolders
				.GetProjectFolders(httpAuthenticator)
				.FirstOrDefault(folder => folder.Name == folderName);

			if (projectFolder == null) projectFolder = ApiRouter.ProjectFolders.InsertProjectFolder(folderName, httpAuthenticator);

			var additionalContent = new Dictionary<string, string>{ ["FolderId"] = projectFolder.Id };
			var response = ApiHelper.SendWithManyPagesRequest<ProjectContainer>(httpAuthenticator, "Projects/GetGroupedProjects",
				HttpMethod.Post,
				additionalContent) ?? new List<ProjectContainer>();

			return response
				.SelectMany(x => x.Projects)
				.ToList();
		}

		public Project CreateProject(HttpAuthenticator httpAuthenticator, dynamic content)
		{
			return ApiHelper.SendDatabaseEntityItem<Project>(content, "Projects/CreateProject", HttpMethod.Post,
				httpAuthenticator);
		}
	}
}
