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
		public static dynamic GetMessageFromRequest(HttpRequestMessage request, HttpAuthenticator httpAuthenticator)
		{
			try
			{
				var response = httpAuthenticator.Client.SendAsync(request).Result;
				response.EnsureSuccessStatusCode();

				return !response.IsSuccessStatusCode ? null : GetMessageFromResponce(response);
			}
			catch (Exception)
			{
				return null;
			}		
		}

		public static dynamic GetMessageFromResponce(HttpResponseMessage response)
		{
			var message = response.Content.ReadAsStringAsync().Result;
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

		public static List<T> SendWithManyPagesRequest<T>(HttpAuthenticator httpAuthenticator, string path, HttpMethod httpMethod, 
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

				var request = CreateRequest(content, $"api/{path}", httpMethod,
					httpAuthenticator.UserCookie);

				var responseMessage = GetMessageFromRequest(request, httpAuthenticator);
				if (responseMessage == null) return null;				

				var newItems = new List<dynamic>(responseMessage.Result)
					.Select(r => (T) Activator.CreateInstance(typeof(T), new object[] { r }));
				resultContainer.AddRange(newItems);

				count += 1;
				if (!(bool) responseMessage.NextPageExists) break;

			} while (true);

			return resultContainer.Count == 0 ? new List<T>() : resultContainer;
		}

		public static T SendDatabaseEntityItem<T>(dynamic content, string path, HttpMethod httpMethod, HttpAuthenticator httpAuthenticator) 
			where T : DatabaseEntityItem, new()
		{		
			var request = CreateRequest((object) content, $"api/{path}", httpMethod,
				httpAuthenticator.UserCookie);

			var responseMessage = GetMessageFromRequest(request, httpAuthenticator).Result;
			if (responseMessage == null) return null;

			return (T) Activator.CreateInstance(typeof(T), new object[] { responseMessage.Name?.ToString() ?? responseMessage.DisplayName.ToString(),
				responseMessage.Id.ToString() });
		}

		public static async Task<bool> TrySendAsync(HttpAuthenticator httpAuthenticator, dynamic content, string path, HttpMethod httpMethod)
		{
			try
			{
				var request = CreateRequest((object)content, $"api/{path}", httpMethod, httpAuthenticator.UserCookie);

				var response = await httpAuthenticator.Client.SendAsync(request);

				return response != null && response.IsSuccessStatusCode;

			}
			catch (Exception)
			{
				return false;
			}		
		}

		public static dynamic SendItemWithParameters(HttpAuthenticator httpAuthenticator, string path, HttpMethod httpMethod, IDictionary<string, string> parameters)
		{
			var request = CreateRequest(parameters, $"api/{path}", httpMethod, httpAuthenticator.UserCookie);

			return GetMessageFromRequest(request, httpAuthenticator).Result;
		}
	}
}
