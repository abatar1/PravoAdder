﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class VisualBlockApi
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

		public List<VisualBlock> Get(HttpAuthenticator httpAuthenticator)
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

		public List<LineType> GetLineTypes(HttpAuthenticator httpAuthenticator)
		{
			var bootstrap = ApiRouter.Bootstrap.GetShellBootstrap(httpAuthenticator);
			IEnumerable<dynamic> participantTypes = bootstrap["ProjectFieldFormats"]["LineTypes"];
			return participantTypes.Select(o => (LineType) JsonConvert.DeserializeObject<LineType>(o.ToString())).ToList();
		}

		public List<ProjectFieldFormat> GetFieldTypes(HttpAuthenticator httpAuthenticator)
		{
			var bootstrap = ApiRouter.Bootstrap.GetShellBootstrap(httpAuthenticator);
			IEnumerable<dynamic> participantTypes = bootstrap["ProjectFieldFormats"]["ProjectFieldFormats"];
			return participantTypes.Select(o => (ProjectFieldFormat) JsonConvert.DeserializeObject<ProjectFieldFormat>(o.ToString())).ToList();
		}

		public List<ProjectFieldFormat> GetEntityTypes(HttpAuthenticator httpAuthenticator)
		{
			var bootstrap = ApiRouter.Bootstrap.GetShellBootstrap(httpAuthenticator);
			IEnumerable<dynamic> participantTypes = bootstrap["ProjectFieldFormats"]["EntityTypes"];
			return participantTypes.Select(o => (ProjectFieldFormat) JsonConvert.DeserializeObject<ProjectFieldFormat>(o.ToString())).ToList();
		}

		public VisualBlock Create(HttpAuthenticator httpAuthenticator, VisualBlock block)
		{
			return ApiHelper.GetItem<VisualBlock>(httpAuthenticator, "VisualBlocks/CreateCustomVisualBlock", HttpMethod.Post, block);
		}
	}
}
