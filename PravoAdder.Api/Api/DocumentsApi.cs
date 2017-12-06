using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class DocumentsApi
	{
		public List<Bulk> Create(HttpAuthenticator httpAuthenticator, List<Bulk> bulk)
		{
			return ApiHelper.GetItem<List<Bulk>>(httpAuthenticator, "Documents/BulkCreate", HttpMethod.Put, bulk);
		}
	}
}
