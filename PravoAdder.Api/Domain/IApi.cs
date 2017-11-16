using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public interface IApi<TResult>
	{
		List<TResult> GetMany(HttpAuthenticator authenticator, string optional = null);
		TResult Get(HttpAuthenticator authenticator, string parameter);
		TResult Create(HttpAuthenticator authenticator, TResult puttingObject);
	}
}
