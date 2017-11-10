using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public interface IGetMany<TResult>
	{
		List<TResult> GetMany(HttpAuthenticator authenticator, string optional = null);
		TResult Get(HttpAuthenticator authenticator, string parameter);
	}
}
