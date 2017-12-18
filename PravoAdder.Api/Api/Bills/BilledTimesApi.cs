using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;
using System.Collections.Generic;
using System.Net.Http;

namespace PravoAdder.Api
{
	public class BilledTimesApi
	{
		public BilledTimes Update(HttpAuthenticator authenticator, BilledTimes billedTimes)
		{
			return ApiHelper.GetItem<BilledTimes>(authenticator, "BilledTimes/Update", HttpMethod.Put, billedTimes);
		}

		public List<BilledTimes> GetMany(HttpAuthenticator httpAuthenticator, string billId)
		{
			var parameters = ApiHelper.CreateParameters(("BillId", billId));
			return ApiHelper.GetItems<BilledTimes>(httpAuthenticator, "BilledTimes/GetBilledTimes", HttpMethod.Post, parameters);
		}
	}
}
