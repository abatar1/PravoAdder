using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Domain.DatabaseEntity;
using PravoAdder.Helpers;

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
