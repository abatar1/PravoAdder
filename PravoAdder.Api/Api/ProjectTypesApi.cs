using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ProjectTypesApi
	{
		public List<ProjectType> GetMany(HttpAuthenticator httpAuthenticator)
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

		public ProjectType PostWithBlocks(HttpAuthenticator httpAuthenticator, string name, string abbreviation)
		{
			var content = new ProjectType
			{
				Name = name,
				Abbreviation = abbreviation,
				VisualBlocks = new List<VisualBlock>()
			};
			return ApiHelper.GetItem<ProjectType>(httpAuthenticator, "ProjectTypes/PostProjectTypeWithBlocks", HttpMethod.Post,
				content);
		}

		public ProjectType PutWithBlocks(HttpAuthenticator httpAuthenticator, ProjectType updatingProjectType)
		{
			return ApiHelper.GetItem<ProjectType>(httpAuthenticator, "ProjectTypes/PutProjectTypeWithBlocks", HttpMethod.Put,
				updatingProjectType);
		}
	}
}
