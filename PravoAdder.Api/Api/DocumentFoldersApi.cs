using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class DocumentFoldersApi
	{
		public DocumentFolder Create(HttpAuthenticator authenticator, DocumentFolder documentFolder)
		{
			return ApiHelper.GetItem<DocumentFolder>(authenticator, "DocumentFolders/CreateDocumentFolder", HttpMethod.Post,
				documentFolder);
		}
	}
}
