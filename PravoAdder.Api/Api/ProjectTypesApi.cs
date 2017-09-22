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
			return ApiHelper.SendWithManyPagesRequest<ProjectType>(httpAuthenticator, "ProjectTypes/GetProjectTypes", HttpMethod.Post);
		}

		public List<VisualBlock> GetVisualBlocks(HttpAuthenticator httpAuthenticator, string projectTypeId)
		{
			var parameters = new Dictionary<string, string> { ["projectTypeId"] = projectTypeId };
			var projectType = ApiHelper.SendDynamicItem(httpAuthenticator, "ProjectTypes/GetProjectType", HttpMethod.Get,
				parameters).VisualBlocks;
			return JsonConvert.DeserializeObject<List<VisualBlock>>(projectType.ToString());
		}
	}
}
