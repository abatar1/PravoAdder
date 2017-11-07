using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ResponsiblesApi
	{
		public List<Responsible> GetMany(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.GetItems<Responsible>(httpAuthenticator, "CompanyUsersSuggest", HttpMethod.Post);
		}
	}
}
