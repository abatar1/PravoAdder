using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PravoAdder.Api
{
	public class RetryHandler : DelegatingHandler
	{
		private static int _maxRetries;

		public RetryHandler(HttpMessageHandler innerHandler, int maxRetries)
			: base(innerHandler)
		{
			_maxRetries = maxRetries;
		}

		protected override async Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			var cancellationTokenSource = new CancellationTokenSource();
			for (var i = 0; i < _maxRetries; i++)
			{
				try
				{
					var responseTask = base.SendAsync(request, cancellationTokenSource.Token);
					if (!responseTask.Wait(TimeSpan.FromMinutes(2)))
					{
						throw new TimeoutException();
					}

					var response = responseTask.Result;
					if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.Forbidden)
					{
						return response;
					}
				}
				catch (TimeoutException)
				{
					cancellationTokenSource.Cancel();
					throw new Exception("Timeout Api error");
				}
				catch (Exception)
				{
					//
				}
				Thread.Sleep(TimeSpan.FromSeconds(10));
			}

			throw new Exception("Api error");
		}
	}
}