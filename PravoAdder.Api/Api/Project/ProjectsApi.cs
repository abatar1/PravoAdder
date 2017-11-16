using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ProjectsApi : IApi<Project>
	{
		public List<Project> GetMany(HttpAuthenticator httpAuthenticator, string folderName = null)
		{
			return GetGroupedMany(httpAuthenticator, folderName).SelectMany(g => g.Projects).ToList();
		}

		public List<GroupedProjects> GetGroupedMany(HttpAuthenticator httpAuthenticator, string folderName = null)
		{
			var projectFolder = ApiRouter.ProjectFolders
				.GetMany(httpAuthenticator)
				.FirstOrDefault(folder => folder.Name == folderName);

			Dictionary<string, string> parameters = null;
			if (projectFolder == null && folderName != null)
			{
				projectFolder = ApiRouter.ProjectFolders.Create(httpAuthenticator, new ProjectFolder {Name = folderName});
				parameters = ApiHelper.CreateParameters(("FolderId", projectFolder.Id));
			}
			else if (projectFolder != null && folderName != null)
			{
				parameters = ApiHelper.CreateParameters(("FolderId", projectFolder.Id));
			}
			return ApiHelper.GetItems<GroupedProjects>(httpAuthenticator, "Projects/GetGroupedProjects",
				HttpMethod.Post, parameters);
		}

		public Project Get(HttpAuthenticator httpAuthenticator, string projectId)
		{
			var parameters = ApiHelper.CreateParameters(("ProjectId", projectId));
			return ApiHelper.GetItem<Project>(httpAuthenticator, "Projects/GetProject", HttpMethod.Get, parameters);
		}

		public Project Put(HttpAuthenticator httpAuthenticator, Project project)
		{
			return ApiHelper.GetItem<Project>(httpAuthenticator, "projects", HttpMethod.Put, project);
		}

		public Project Create(HttpAuthenticator httpAuthenticator, Project project)
		{
			return ApiHelper.GetItem<Project>(httpAuthenticator, "Projects/CreateProject", HttpMethod.Post, project);
		}

		public void Delete(HttpAuthenticator httpAuthenticator, string projectId)
		{
			var parameters = ApiHelper.CreateParameters(("Id", projectId));
			ApiHelper.SendItem(httpAuthenticator, "Projects/DeleteProject", HttpMethod.Delete, parameters);
		}

		public void Archive(HttpAuthenticator httpAuthenticator, string projectId)
		{
			var parameters = ApiHelper.CreateParameters(("Id", projectId));
			ApiHelper.SendItem(httpAuthenticator, "projects/Archive", HttpMethod.Put, parameters);
		}

		public void Restore(HttpAuthenticator httpAuthenticator, string projectId)
		{
			var parameters = ApiHelper.CreateParameters(("Id", projectId));
			ApiHelper.SendItem(httpAuthenticator, "Projects/Restore", HttpMethod.Put, parameters);
		}
	}
}
