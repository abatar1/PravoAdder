using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Domain;

namespace PravoAdder.Readers
{
	public abstract class Creator
	{
		protected Creator(HttpAuthenticator httpAuthenticator, Settings settings)
		{
			HttpAuthenticator = httpAuthenticator;
			Settings = settings;
		}

		protected HttpAuthenticator HttpAuthenticator { get; }
		protected Settings Settings { get; }

		public abstract ICreatable Create(Row header, Row row, DatabaseEntityItem item = null);
	}
}
