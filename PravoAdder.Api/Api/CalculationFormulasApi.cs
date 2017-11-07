using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class CalculationFormulasApi
	{
		public IList<CalculationFormula> Get(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.GetItems<CalculationFormula>(httpAuthenticator, "CalculationFormulasSuggest/GetCalculationFormulas", HttpMethod.Post);
		}

		public int GetInputData(HttpAuthenticator httpAuthenticator, string projectId, string calculationId, string visualBlockId, string blockLineId)
		{
			var parameters = ApiHelper.CreateParameters(("ProjectId", projectId), ("CalculationFormulaId", calculationId),
				("VisualBlockId", visualBlockId), ("BlockLineId", blockLineId));
			return (int) ApiHelper.GetItem(httpAuthenticator, "Calculation/GetInputData", HttpMethod.Get, parameters).Result.Result;
		}
	}
}
