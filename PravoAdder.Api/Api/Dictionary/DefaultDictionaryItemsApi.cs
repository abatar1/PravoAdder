using System;
using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api.Api
{
	public class DefaultDictionaryItemsApi : IApi<DictionaryItem>
	{
		public List<DictionaryItem> GetMany(HttpAuthenticator httpAuthenticator, string dictionaryName)
		{
			return ApiHelper.GetItems<DictionaryItem>(httpAuthenticator, "dictionary/getdictionaryitems", HttpMethod.Post,
				ApiHelper.CreateParameters(("SystemName", dictionaryName)));
		}

		public DictionaryItem Get(HttpAuthenticator authenticator, string parameter)
		{
			throw new NotImplementedException();
		}

		public DictionaryItem Create(HttpAuthenticator authenticator, DictionaryItem puttingObject)
		{
			throw new NotImplementedException();
		}
	}
}
