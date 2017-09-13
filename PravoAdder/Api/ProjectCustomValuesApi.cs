﻿using System.Net.Http;
using System.Threading.Tasks;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Helpers;

namespace PravoAdder.Api
{
	public class ProjectCustomValuesApi
	{
		public async Task<bool> Create(HttpAuthenticator httpAuthenticator, dynamic content)
		{
			return await ApiHelper.TrySendAsync(httpAuthenticator, content, "ProjectCustomValues/Create", HttpMethod.Post);
		}
	}
}
