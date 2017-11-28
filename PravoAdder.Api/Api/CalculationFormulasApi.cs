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

		public double GetInputData(HttpAuthenticator httpAuthenticator, string calculationId, string visualBlockId,
			string blockLineId, string projectVisualBlockId, string entityId)
		{
			var parameters = ApiHelper.CreateParameters(("CalculationFormulaId", calculationId),
				("VisualBlockId", visualBlockId), ("BlockLineId", blockLineId), ("ProjectVisualBlockId", projectVisualBlockId), ("EntityId", entityId));
			return (double) ApiHelper.GetItem(httpAuthenticator, "Calculation/GetInputData", HttpMethod.Get, parameters).Result.Result;
		}
	}
}
