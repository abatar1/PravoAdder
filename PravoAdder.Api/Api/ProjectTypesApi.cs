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
			var parameters = new Dictionary<string, string> { ["projectTypeId"] = projectTypeId };
			var projectType = ApiHelper.GetItem(httpAuthenticator, "ProjectTypes/GetProjectType", HttpMethod.Get,
				parameters).Result.VisualBlocks;
			return JsonConvert.DeserializeObject<List<VisualBlock>>(projectType.ToString());
		}

		public VisualBlock GetEntityCardVisualBlock(HttpAuthenticator httpAuthenticator, string entityId, string entityTypeName)
		{
			var content = new
			{
				EntityId = entityId,
				EntityTypeSysName = entityTypeName
			};
			var response = ApiHelper.GetItem(httpAuthenticator, "EntityVisualBlocks/GetEntityCardVisualBlock",
				HttpMethod.Post, content);

			return JsonConvert.DeserializeObject<VisualBlock>(response.ToString());
		}
	}
}
