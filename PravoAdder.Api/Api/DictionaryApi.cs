using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class DictionaryApi : IGetMany<DictionaryInfo>
	{
		public DictionaryItem Put(HttpAuthenticator httpAuthenticator, string sysName, string itemName)
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
			return ApiHelper.GetItems<DictionaryItem>(httpAuthenticator, "dictionary/getdictionaryitems", HttpMethod.Post,
				ApiHelper.CreateParameters(("SystemName", dictionaryName)));
		}

		public List<DictionaryInfo> GetMany(HttpAuthenticator httpAuthenticator, string optional = null)
		{
			return ApiHelper.GetItems<DictionaryInfo>(httpAuthenticator, "dictionary/GetDictionaryList", HttpMethod.Post);
		}

		public DictionaryInfo Get(HttpAuthenticator authenticator, string parameter)
		{
			throw new System.NotImplementedException();
		}

		public DictionaryInfo Create(HttpAuthenticator httpAuthenticator, DictionaryInfo dictionary)
		{
			if (dictionary.Items == null) dictionary.Items = new List<DictionaryItem>();
			return ApiHelper.GetItem<DictionaryInfo>(httpAuthenticator, "dictionary/CreateDictionary", HttpMethod.Post, dictionary);
		}
	}
}
