using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class BootstrapApi
	{
		public dynamic GetBootstrap(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.GetItem(httpAuthenticator, "bootstrap/GetBootstrap", HttpMethod.Get, new Dictionary<string, string>());
		}

		public dynamic GetShellBootstrap(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.GetItem(httpAuthenticator, "bootstrap/GetShellBootstrap", HttpMethod.Get, new Dictionary<string, string>());
		}
	}
}
