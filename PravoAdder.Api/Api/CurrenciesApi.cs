﻿using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class CurrenciesApi : IGetMany<DictionaryItem>
	{
		public List<DictionaryItem> GetMany(HttpAuthenticator httpAuthenticator, string optional = null)
		{
			return ApiHelper.GetItems<DictionaryItem>(httpAuthenticator, "Currencies/GetCurrencies", HttpMethod.Post);
		}

		public DictionaryItem Get(HttpAuthenticator authenticator, string parameter)
		{
			throw new System.NotImplementedException();
		}
	}
}
