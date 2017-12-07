using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class VisualBlockApi : IApi<VisualBlockModel>
	{
		public VisualBlockModel GetEntityCard(HttpAuthenticator httpAuthenticator, string entityId, string entityTypeId)
		{
			var content = new
			{
				EntityId = entityId,
				EntityTypeSysName = entityTypeId
			};
			return ApiHelper.GetItem<VisualBlockModel>(httpAuthenticator, "EntityVisualBlocks/GetEntityCardVisualBlock",
				HttpMethod.Post, content);
		}

		public List<VisualBlockModel> GetMany(HttpAuthenticator httpAuthenticator, string optional = null)
		{
			return ApiHelper.GetItems<VisualBlockModel>(httpAuthenticator, "VisualBlocks/GetCustomVisualBlocks", HttpMethod.Post);
		}

		public VisualBlockModel Get(HttpAuthenticator httpAuthenticator, string id)
		{
			return ApiHelper.GetItem<VisualBlockModel>(httpAuthenticator, "VisualBlocks/GetCustomVisualBlocks", HttpMethod.Post,
				ApiHelper.CreateParameters(("id", id)));
		}

		public VisualBlockModel Update(HttpAuthenticator httpAuthenticator, VisualBlockModel updatedBlock)
		{
			return ApiHelper.GetItem<VisualBlockModel>(httpAuthenticator, "VisualBlocks/UpdateCustomVisualBlock", HttpMethod.Put, updatedBlock);
		}		

		public VisualBlockModel Create(HttpAuthenticator httpAuthenticator, VisualBlockModel block)
		{
			return ApiHelper.GetItem<VisualBlockModel>(httpAuthenticator, "VisualBlocks/CreateCustomVisualBlock", HttpMethod.Post, block);
		}
	}
}
