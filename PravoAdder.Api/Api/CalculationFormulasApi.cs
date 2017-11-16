using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class CalculationFormulasApi : IApi<CalculationFormula>
	{
		public List<CalculationFormula> GetMany(HttpAuthenticator httpAuthenticator, string optional = null)
		{
			return ApiHelper.GetItems<CalculationFormula>(httpAuthenticator, "CalculationFormulasSuggest/GetCalculationFormulas", HttpMethod.Post);
		}

		public CalculationFormula Get(HttpAuthenticator authenticator, string parameter)
		{
			throw new System.NotImplementedException();
		}

		public CalculationFormula Create(HttpAuthenticator authenticator, CalculationFormula puttingObject)
		{
			throw new System.NotImplementedException();
		}

		public int GetInputData(HttpAuthenticator httpAuthenticator, string projectId, string calculationId, string visualBlockId, string blockLineId)
		{
			var parameters = ApiHelper.CreateParameters(("ProjectId", projectId), ("CalculationFormulaId", calculationId),
				("VisualBlockId", visualBlockId), ("BlockLineId", blockLineId));
			return (int) ApiHelper.GetItem(httpAuthenticator, "Calculation/GetInputData", HttpMethod.Get, parameters).Result.Result;
		}
	}
}
