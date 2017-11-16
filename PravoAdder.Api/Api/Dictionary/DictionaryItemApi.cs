using System;
using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api.Api
{
	public class DictionaryItemApi : IApi<DictionaryItem>
	{
		public DictionaryItem Create(HttpAuthenticator httpAuthenticator, DictionaryItem item)
		{
			if (item.SystemName == null || item.Name == null) return null;

			return ApiHelper.GetItem<DictionaryItem>(httpAuthenticator, "Dictionary/SaveDictionaryItem", HttpMethod.Put, item);
		}

		public List<DictionaryItem> GetMany(HttpAuthenticator httpAuthenticator, string dictionaryName)
		{
			return ApiHelper.GetItems<DictionaryItem>(httpAuthenticator, $"dictionary/{dictionaryName}/getdictionaryitems", HttpMethod.Post);
		}

		public DictionaryItem Get(HttpAuthenticator authenticator, string parameter)
		{
			throw new NotImplementedException();
		}
	}
}
