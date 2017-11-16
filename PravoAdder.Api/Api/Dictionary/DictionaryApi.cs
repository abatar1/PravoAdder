using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class DictionaryApi : IApi<DictionaryInfo>
	{
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
