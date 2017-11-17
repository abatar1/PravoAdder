using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Domain;

namespace PravoAdder.Readers
{
	public abstract class Creator
	{
		protected Creator(HttpAuthenticator httpAuthenticator, ApplicationArguments applicationArguments)
		{
			HttpAuthenticator = httpAuthenticator;
			ApplicationArguments = applicationArguments;
		}

		protected HttpAuthenticator HttpAuthenticator { get; }
		protected ApplicationArguments ApplicationArguments { get; }

		public abstract ICreatable Create(Row header, Row row, DatabaseEntityItem item = null);
	}
}
