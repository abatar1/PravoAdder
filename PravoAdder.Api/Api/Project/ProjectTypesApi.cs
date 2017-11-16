using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ProjectTypesApi : IApi<ProjectType>
	{
		public List<ProjectType> GetMany(HttpAuthenticator httpAuthenticator, string optional = null)
		{
			return ApiHelper.GetItems<ProjectType>(httpAuthenticator, "ProjectTypes/GetProjectTypes", HttpMethod.Post);
		}

		public ProjectType Get(HttpAuthenticator httpAuthenticator, string projectTypeId)
		{
			var parameters = ApiHelper.CreateParameters(("projectTypeId", projectTypeId));
			return ApiHelper.GetItem<ProjectType>(httpAuthenticator, "ProjectTypes/GetProjectType", HttpMethod.Get, parameters);
		}

		public List<VisualBlock> GetVisualBlocks(HttpAuthenticator httpAuthenticator, string projectTypeId)
		{
			var parameters = ApiHelper.CreateParameters(("projectTypeId", projectTypeId));
			return ApiHelper.GetItem<VisualBlockWrapper>(httpAuthenticator, "ProjectTypes/GetProjectType", HttpMethod.Get,
				parameters).VisualBlocks;
		}

		public ProjectType Create(HttpAuthenticator httpAuthenticator, ProjectType projectType)
		{
			return ApiHelper.GetItem<ProjectType>(httpAuthenticator, "ProjectTypes/PostProjectTypeWithBlocks", HttpMethod.Post,
				projectType);
		}

		public ProjectType Update(HttpAuthenticator httpAuthenticator, ProjectType updatingProjectType)
		{
			return ApiHelper.GetItem<ProjectType>(httpAuthenticator, "ProjectTypes/PutProjectTypeWithBlocks", HttpMethod.Put,
				updatingProjectType);
		}
	}
}
