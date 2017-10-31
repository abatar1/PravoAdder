using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ProjectTypesApi
	{
		public IList<ProjectType> GetProjectTypes(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.GetItems<ProjectType>(httpAuthenticator, "ProjectTypes/GetProjectTypes", HttpMethod.Post);
		}

		public List<VisualBlock> GetVisualBlocks(HttpAuthenticator httpAuthenticator, string projectTypeId)
		{
			var parameters = ApiHelper.CreateParameters(("projectTypeId", projectTypeId));
			return ApiHelper.GetItem<VisualBlockWrapper>(httpAuthenticator, "ProjectTypes/GetProjectType", HttpMethod.Get,
				parameters).VisualBlocks;
		}		
	}
}
