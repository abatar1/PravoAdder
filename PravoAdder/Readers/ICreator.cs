using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Domain;

namespace PravoAdder.Readers
{
	public interface ICreator
	{
		HttpAuthenticator HttpAuthenticator { get; }
		ICreatable Create(Row header, Row row);
	}
}
