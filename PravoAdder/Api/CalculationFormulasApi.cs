using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Domain.DatabaseEntity;
using PravoAdder.Helpers;

namespace PravoAdder.Api
{
	public class CalculationFormulasApi
	{
		public IList<CalculationFormula> GetCalculationFormulas(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.SendWithManyPagesRequest<CalculationFormula>(httpAuthenticator, "CalculationFormulasSuggest/GetCalculationFormulas", HttpMethod.Post);
		}
	}
}
