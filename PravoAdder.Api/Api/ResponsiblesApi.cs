using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ResponsiblesApi : IApi<Responsible>
	{
		public List<Responsible> GetMany(HttpAuthenticator httpAuthenticator, string optional = null)
		{
			return ApiHelper.GetItems<Responsible>(httpAuthenticator, "CompanyUsersSuggest", HttpMethod.Post);
		}

		public Responsible Get(HttpAuthenticator authenticator, string parameter)
		{
			throw new System.NotImplementedException();
		}

		public Responsible Create(HttpAuthenticator authenticator, Responsible puttingObject)
		{
			throw new System.NotImplementedException();
		}
	}
}
