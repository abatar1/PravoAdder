using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class CalculationFormulasApi
	{
		public IList<CalculationFormula> GetCalculationFormulas(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.GetItems<CalculationFormula>(httpAuthenticator, "CalculationFormulasSuggest/GetCalculationFormulas", HttpMethod.Post);
		}
	}
}
