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
	        for (var i = 0; i < _maxRetries; i++)
	        {
				try
		        {
			        var response = await base.SendAsync(request, CancellationToken.None).ConfigureAwait(false);
			        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.Forbidden)
			        {
				        return response;
			        }
		        }
		        catch (Exception)
		        {
			        return null;
		        }
		        Thread.Sleep(TimeSpan.FromSeconds(10));
			}
	        return null;
        }
    }
}