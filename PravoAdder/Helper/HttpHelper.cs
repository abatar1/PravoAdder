using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PravoAdder.Helper
{
    public static class HttpHelper
    {
        public static async Task<dynamic> GetMessageFromResponce(HttpResponseMessage response)
        {
            var message = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject(message);
        }

        public static HttpRequestMessage CreateRequest(object content, string requestUri, HttpMethod method, Cookie cookie)
        {
            var serializedContent = JsonConvert.SerializeObject(content);
            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = new StringContent(serializedContent, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Cookie", cookie.ToString());

            return request;
        }

        public static async Task<string> GetContentId(HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JObject.Parse(responseContent)["Result"]["Id"].ToString();
        }
    }
}
