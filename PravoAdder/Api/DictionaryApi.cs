using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Domain;
using PravoAdder.Helpers;

namespace PravoAdder.Api
{
	public class DictionaryApi
	{
		public DictionaryItem SaveDictionaryItem(HttpAuthenticator httpAuthenticator, dynamic content)
		{
			return ApiHelper.SendDatabaseEntityItem<ProjectFolder>(content, "Dictionary/SaveDictionaryItem", HttpMethod.Put,
				httpAuthenticator);
		}

		public IList<DictionaryItem> GetDictionaryItems(HttpAuthenticator httpAuthenticator, string dictionaryName)
		{
			return ApiHelper.SendWithManyPagesRequest<DictionaryItem>(httpAuthenticator, $"dictionary/{dictionaryName}/getdictionaryitems", HttpMethod.Post);
		}
	}
}
