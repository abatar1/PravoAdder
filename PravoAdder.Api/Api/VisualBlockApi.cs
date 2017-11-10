using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class VisualBlockApi : IGetMany<VisualBlock>
	{
		public VisualBlock GetEntityCard(HttpAuthenticator httpAuthenticator, string entityId, string entityTypeId)
		{
			var content = new
			{
				EntityId = entityId,
				EntityTypeSysName = entityTypeId
			};
			return ApiHelper.GetItem<VisualBlock>(httpAuthenticator, "EntityVisualBlocks/GetEntityCardVisualBlock",
				HttpMethod.Post, content);
		}

		public List<VisualBlock> GetMany(HttpAuthenticator httpAuthenticator, string optional = null)
		{
			return ApiHelper.GetItems<VisualBlock>(httpAuthenticator, "VisualBlocks/GetCustomVisualBlocks", HttpMethod.Post);
		}

		public VisualBlock Get(HttpAuthenticator httpAuthenticator, string id)
		{
			return ApiHelper.GetItem<VisualBlock>(httpAuthenticator, "VisualBlocks/GetCustomVisualBlocks", HttpMethod.Post,
				ApiHelper.CreateParameters(("id", id)));
		}

		public VisualBlock Update(HttpAuthenticator httpAuthenticator, VisualBlock updatedBlock)
		{
			return ApiHelper.GetItem<VisualBlock>(httpAuthenticator, "VisualBlocks/UpdateCustomVisualBlock", HttpMethod.Put, updatedBlock);
		}		

		public VisualBlock Create(HttpAuthenticator httpAuthenticator, VisualBlock block)
		{
			return ApiHelper.GetItem<VisualBlock>(httpAuthenticator, "VisualBlocks/CreateCustomVisualBlock", HttpMethod.Post, block);
		}
	}
}
