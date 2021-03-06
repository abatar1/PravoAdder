﻿using System.Net.Http;
using System.Threading.Tasks;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class CasebookApi
	{
		public async Task<bool> CheckAsync(HttpAuthenticator httpAuthenticator, string projectId, string syncNumber)
		{
			var content = new
			{
				ProjectId = projectId,
				CasebookNumber = syncNumber
			};

			return await ApiHelper.TrySendAsync(httpAuthenticator, "Casebook/CheckCasebookCase", HttpMethod.Put, content);
		}
	}
}
