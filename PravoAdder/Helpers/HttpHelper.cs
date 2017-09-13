using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PravoAdder.Helpers
{
    public static class HttpHelper
    {
        public static async Task<dynamic> GetMessageFromResponceAsync(HttpResponseMessage response)
        {
            var message = await response.Content.ReadAsStringAsync();
            return message == null ? null : JsonConvert.DeserializeObject(message);
        }

        public static HttpRequestMessage CreateRequest(object content, string requestUri, HttpMethod method,
            Cookie cookie)
        {
	        HttpRequestMessage request;
			if (content is IDictionary<string, string> dictionary)
			{
				var parametersBuilder = new StringBuilder();
				foreach (var parameter in dictionary)
				{
					parametersBuilder.Append($"{parameter.Key}={parameter.Value}&");
				}		
				
				var parametersString = parametersBuilder.ToString().Remove(parametersBuilder.Length - 1);
				request = new HttpRequestMessage(method, $"{requestUri}?{parametersString}");
			}
			else
			{
				var serializedContent = JsonConvert.SerializeObject(content);
				if (serializedContent == null) return null;

				request = new HttpRequestMessage(method, requestUri)
				{
					Content = new StringContent(serializedContent, Encoding.UTF8, "application/json")
				};
			}

			request.Headers.Add("Cookie", cookie.ToString());

            return request;
        }

        public static async Task<string> GetContentIdAsync(HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent == null ? null : JObject.Parse(responseContent)["Result"]["Id"].ToString();
        }
    }
}