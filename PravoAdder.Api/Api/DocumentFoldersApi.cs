using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class DocumentFoldersApi
	{
		public VirtualCatalogItem Create(HttpAuthenticator authenticator, VirtualCatalogItem documentFolder)
		{
			return ApiHelper.GetItem<VirtualCatalogItem>(authenticator, "DocumentFolders/CreateDocumentFolder", HttpMethod.Post,
				documentFolder);
		}
	}
}
