using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Domain;
using PravoAdder.Domain.Info;
using PravoAdder.Helpers;

namespace PravoAdder.Api
{
	public class ProjectsApi
	{
		public IList<Project> GetProjects(HttpAuthenticator httpAuthenticator, HeaderBlockInfo header, string projectGroupid)
		{
			var projectFolder = Api.ProjectFolders
				.GetProjectFolders(httpAuthenticator)
				.FirstOrDefault(folder => folder.Name == header.FolderName);

			if (projectFolder == null) projectFolder = Api.ProjectFolders.InsertProjectFolder(header.FolderName, httpAuthenticator);

			var additionalContent = new Dictionary<string, string>{ ["FolderId"] = projectFolder.Id };
			return ApiHelper.SendWithManyPagesRequest<Project>(httpAuthenticator, "Projects/GetGroupedProjects", HttpMethod.Post,
				additionalContent);
		}

		public Project CreateProject(HttpAuthenticator httpAuthenticator, dynamic content)
		{
			return ApiHelper.SendDatabaseEntityItem<ProjectFolder>(content, "Projects/CreateProject", HttpMethod.Post,
				httpAuthenticator);
		}
	}
}
