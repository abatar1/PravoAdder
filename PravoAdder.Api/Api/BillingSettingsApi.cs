using System.Net.Http;
using PravoAdder.Api.Domain.Other;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class BillingSettingsApi
	{
		public BillingSettings Get(HttpAuthenticator authenticator)
		{
			return ApiHelper.GetItem<BillingSettings>(authenticator, "BillingSettings", HttpMethod.Get, null);
		}

		public BillingSettings Put(HttpAuthenticator authenticator, BillingSettings billingSettings)
		{
			return ApiHelper.GetItem<BillingSettings>(authenticator, "BillingSettings", HttpMethod.Put, billingSettings);
		}
	}
}
