using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Domain;
using PravoAdder.Helpers;

namespace PravoAdder.Api
{
	public class ProjectTypesApi
	{
		public IList<ProjectType> GetProjectTypes(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.SendWithManyPagesRequest<ProjectType>(httpAuthenticator, "ProjectTypes/GetProjectTypes", HttpMethod.Post);
		}

		public dynamic GetVisualBlocks(HttpAuthenticator httpAuthenticator, string projectTypeId)
		{
			var parameters = new Dictionary<string, string> { ["projectTypeId"] = projectTypeId };
			var projectType = ApiHelper.SendDynamicItem(httpAuthenticator, "ProjectTypes/GetProjectType", HttpMethod.Get,
				parameters);
			return new List<dynamic>(projectType)
				.First(block => block.Name == "VisualBlocks").Value;
		}
	}
}
