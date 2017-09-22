using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ResponsiblesApi
	{
		public IList<Responsible> GetResponsibles(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.SendWithManyPagesRequest<Responsible>(httpAuthenticator, "CompanyUsersSuggest", HttpMethod.Post);
		}
	}
}
