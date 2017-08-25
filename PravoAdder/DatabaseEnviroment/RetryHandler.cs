using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PravoAdder.DatabaseEnviroment
{
	public class RetryHandler : DelegatingHandler
	{
		private const int MaxRetries = 10;

		public RetryHandler(HttpMessageHandler innerHandler)
			: base(innerHandler)
		{ }

		protected override async Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			HttpResponseMessage response = null;

			for (var i = 0; i < MaxRetries; i++)
			{
				response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
				if (response.IsSuccessStatusCode)
				{
					return response;
				}

				Thread.Sleep(TimeSpan.FromSeconds(30));
			}

			return response;
		}
	}
}
