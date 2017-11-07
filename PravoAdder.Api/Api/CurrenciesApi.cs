using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class CurrenciesApi
	{
		public List<DictionaryItem> GetMany(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.GetItems<DictionaryItem>(httpAuthenticator, "Currencies/GetCurrencies", HttpMethod.Post);
		}
	}
}
