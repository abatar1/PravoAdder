using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class VirtualCatalogApi
	{
		public List<DocumentFolder> GetMany(HttpAuthenticator httpAuthenticator, string folderId)
		{
			var parameters = ApiHelper.CreateParameters(("FolderId", folderId));
			return ApiHelper.GetItems<DocumentFolder>(httpAuthenticator, "VirtualCatalog/GetContent", HttpMethod.Post,
				parameters);
		}
	}
}
