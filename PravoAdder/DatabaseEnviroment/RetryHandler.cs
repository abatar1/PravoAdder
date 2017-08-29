using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PravoAdder.DatabaseEnviroment
{
	public class RetryHandler : DelegatingHandler
	{
		private const int MaxRetries = 5;

		public RetryHandler(HttpMessageHandler innerHandler)
			: base(innerHandler)
		{ }

		protected override async Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			for (var i = 0; i < MaxRetries; i++)
			{
				try
				{
					var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
					return response;
				}
				catch (Exception)
				{
					Thread.Sleep(TimeSpan.FromSeconds(30));
				}				
			}
			return null;
		}
	}
}
