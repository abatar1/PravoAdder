using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class DictionaryApi
	{
		public DictionaryItem SaveItem(HttpAuthenticator httpAuthenticator, string sysName, string itemName)
		{
			var content = new
			{
				SystemName = sysName,
				Name = itemName
			};

			return ApiHelper.GetItem<DictionaryItem>(httpAuthenticator, "Dictionary/SaveDictionaryItem", HttpMethod.Put,
				content);
		}

		public List<DictionaryItem> GetItems(HttpAuthenticator httpAuthenticator, string dictionaryName)
		{
			return ApiHelper.GetItems<DictionaryItem>(httpAuthenticator, $"dictionary/{dictionaryName}/getdictionaryitems", HttpMethod.Post);
		}

		public IList<DictionaryItem> GetDefaultItems(HttpAuthenticator httpAuthenticator, string dictionaryName)
		{
			var content = new Content(ApiHelper.CreateParameters(("SystemName", dictionaryName)));
			return ApiHelper.GetItems<DictionaryItem>(httpAuthenticator, "dictionary/getdictionaryitems", HttpMethod.Post, content);
		}

		public List<DictionaryInfo> GetMany(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.GetItems<DictionaryInfo>(httpAuthenticator, "dictionary/GetDictionaryList", HttpMethod.Post);
		}

		public DictionaryInfo Create(HttpAuthenticator httpAuthenticator, DictionaryInfo dictionary)
		{
			if (dictionary.Items == null) dictionary.Items = new List<DictionaryItem>();
			return ApiHelper.GetItem<DictionaryInfo>(httpAuthenticator, "dictionary/CreateDictionary", HttpMethod.Post, dictionary);
		}
	}
}
