using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class DictionaryApi
	{
		public DictionaryItem SaveDictionaryItem(HttpAuthenticator httpAuthenticator, string sysName, string itemName)
		{
			var content = new
			{
				SystemName = sysName,
				Name = itemName
			};

			return ApiHelper.GetItem<DictionaryItem>(content, "Dictionary/SaveDictionaryItem", HttpMethod.Put,
				httpAuthenticator);
		}

		public List<DictionaryItem> GetDictionaryItems(HttpAuthenticator httpAuthenticator, string dictionaryName)
		{
			return ApiHelper.GetItems<DictionaryItem>(httpAuthenticator, $"dictionary/{dictionaryName}/getdictionaryitems", HttpMethod.Post);
		}
	}
}
