using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class BootstrapApi
	{
		public dynamic Get(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.GetItem(httpAuthenticator, "bootstrap/GetBootstrap", HttpMethod.Get, new Dictionary<string, string>());
		}

		public dynamic GetShell(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.GetItem(httpAuthenticator, "bootstrap/GetShellBootstrap", HttpMethod.Get, new Dictionary<string, string>());
		}
	}
}
