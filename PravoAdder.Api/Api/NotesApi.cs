using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class NotesApi
	{
		public void Create(HttpAuthenticator authenticator, Note note)
		{
			ApiHelper.SendItem(authenticator, "Notes/Create", HttpMethod.Post, note);
		}
	}
}
