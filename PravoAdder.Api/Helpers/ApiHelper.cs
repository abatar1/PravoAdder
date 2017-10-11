using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using PravoAdder.Api.Domain;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace PravoAdder.Api.Helpers
{
	public class ApiHelper
	{
		public static dynamic GetResponseFromRequest(HttpRequestMessage request, HttpAuthenticator httpAuthenticator)
		{
			try
			{
				var response = httpAuthenticator.Client.SendAsync(request).Result;
				response.EnsureSuccessStatusCode();

				return !response.IsSuccessStatusCode ? null : ReadFromResponce(response);
			}
			catch (Exception)
			{
				return null;
			}		
		}

		public static dynamic ReadFromResponce(HttpResponseMessage response)
		{
			var message = response.Content.ReadAsStringAsync().Result;
			return message == null ? null : JsonConvert.DeserializeObject(message);
		}

		public static HttpRequestMessage CreateHttpRequest(object content, string requestUri, HttpMethod method,
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

		public static List<T> GetItems<T>(HttpAuthenticator httpAuthenticator, string path, HttpMethod httpMethod, 
			IDictionary<string, string> additionalContent = null)
		{
			var count = 1;
			var resultContainer = new List<T>();
			do
			{
				var content = new ExpandoObject() as IDictionary<string, object>;			
				content.Add("PageSize", ApiRouter.PageSize);
				content.Add("Page", count);
				if (additionalContent != null)
				{
					foreach (var pair in additionalContent)
					{
						content.Add(pair.Key, pair.Value);
					}
				}

				var request = CreateHttpRequest(content, $"api/{path}", httpMethod,
					httpAuthenticator.UserCookie);

				var responseMessage = GetResponseFromRequest(request, httpAuthenticator);
				if (responseMessage == null) return null;				

				var newItems = new List<dynamic>(responseMessage.Result)
					.Select(r => (T) Activator.CreateInstance(typeof(T), new object[] { r }));
				resultContainer.AddRange(newItems);

				count += 1;
				if (!(bool) responseMessage.NextPageExists) break;

			} while (true);

			return resultContainer.Count == 0 ? new List<T>() : resultContainer;
		}	

		public static async Task<bool> TrySendAsync(HttpAuthenticator httpAuthenticator, dynamic content, string path, HttpMethod httpMethod)
		{
			try
			{
				var request = CreateHttpRequest((object)content, $"api/{path}", httpMethod, httpAuthenticator.UserCookie);
				var response = await httpAuthenticator.Client.SendAsync(request);
				return response != null && response.IsSuccessStatusCode;

			}
			catch (Exception)
			{
				return false;
			}		
		}

		public static T GetItem<T>(dynamic content, string path, HttpMethod httpMethod, HttpAuthenticator httpAuthenticator)
			where T : DatabaseEntityItem, new()
		{
			var item = GetItem(httpAuthenticator, path, httpMethod, content);
			return (T) Activator.CreateInstance(typeof(T), new object[] {item});
		}

		public static dynamic GetItem(HttpAuthenticator httpAuthenticator, string path, HttpMethod httpMethod, IDictionary<string, string> parameters)
		{
			var request = CreateHttpRequest(parameters, $"api/{path}", httpMethod, httpAuthenticator.UserCookie);
			return GetResponseFromRequest(request, httpAuthenticator).Result;
		}

		public static dynamic GetItem(HttpAuthenticator httpAuthenticator, string path, HttpMethod httpMethod, dynamic content)
		{
			var request = CreateHttpRequest(content, $"api/{path}", httpMethod, httpAuthenticator.UserCookie);
			return GetResponseFromRequest(request, httpAuthenticator).Result;
		}
	}
}
