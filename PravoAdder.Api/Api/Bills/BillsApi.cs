using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class BillsApi
	{
		public Bill Create(HttpAuthenticator authenticator, string projectId)
		{
			var content = new
			{
				ProjectId = projectId
			};
			return ApiHelper.GetItem<Bill>(authenticator, "Bills/Create", HttpMethod.Put, content);
		}

		public void UpdateStatus(HttpAuthenticator authenticator, BillStatusGroup billStatus)
		{
			ApiHelper.SendItem(authenticator, "BillGroupActions/UpdateBillsStatus", HttpMethod.Put, billStatus);
		}

		public bool HasCaseUnbilledTimes(HttpAuthenticator authenticator, string projectId)
		{
			var parameter = ApiHelper.CreateParameters(("CaseId", projectId));
			return ApiHelper.GetItem<bool>(authenticator, "Bills/HasCaseUnbilledTimes", HttpMethod.Get, parameter);
		}

		public Bill Get(HttpAuthenticator authenticator, string billId)
		{
			var parameter = ApiHelper.CreateParameters(("BillId", billId));
			return ApiHelper.GetItem<Bill>(authenticator, "Bills/GetBill", HttpMethod.Get, parameter);
		}

		public Bill Rebuild(HttpAuthenticator authenticator, string billId)
		{
			var content = new
			{
				BillId = billId
			};
			return ApiHelper.GetItem<Bill>(authenticator, "Bills/Rebuild", HttpMethod.Put, content);
		}
	}
}
