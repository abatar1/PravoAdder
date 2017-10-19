using System;
using System.Collections.Generic;
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

		public static async Task<bool> TrySendAsync(HttpAuthenticator httpAuthenticator, string path, HttpMethod httpMethod, object content)
		{
			try
			{
				var request = CreateHttpRequest(content, $"api/{path}", httpMethod, httpAuthenticator.UserCookie);
				var response = await httpAuthenticator.Client.SendAsync(request);
				return response != null && response.IsSuccessStatusCode;

			}
			catch (Exception)
			{
				return false;
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
			if (content == null) throw new ArgumentException("Cannot create http json request because content is null.");

			HttpRequestMessage request;
			if (content is IDictionary<string, string> dictionary)
			{
				var parametersString = string.Empty;
				if (dictionary.Count > 0)
				{
					var parametersBuilder = new StringBuilder();
					foreach (var parameter in dictionary)
					{
						parametersBuilder.Append($"{parameter.Key}={parameter.Value}&");
					}
					parametersString = parametersBuilder.ToString().Remove(parametersBuilder.Length - 1);
				}
				
				request = new HttpRequestMessage(method, $"{requestUri}?{parametersString}");
			}
			else
			{
				request = new HttpRequestMessage(method, requestUri);
				var serializedContent = JsonConvert.SerializeObject(content);
				{
					request.Content = new StringContent(serializedContent, Encoding.UTF8, "application/json");
				}				
			}

			request.Headers.Add("Cookie", cookie.ToString());

			return request;
		}

		public static List<T> GetItems<T>(HttpAuthenticator httpAuthenticator, string path, HttpMethod httpMethod, Content content = null)
		{
			var count = 1;
			var resultContainer = new List<T>();
			do
			{
				if (content == null) content = new Content();
				var request = CreateHttpRequest(content.Get(count), $"api/{path}", httpMethod,
					httpAuthenticator.UserCookie);

				var responseMessage = GetResponseFromRequest(request, httpAuthenticator);
				if (responseMessage == null) return null;				

				var newItems = new List<object>(responseMessage.Result)
					.Select(r => (T) Activator.CreateInstance(typeof(T), r));
				resultContainer.AddRange(newItems);

				count += 1;
				if (!(bool) responseMessage.NextPageExists) break;

			} while (true);

			return resultContainer.Count == 0 ? new List<T>() : resultContainer;
		}	

		public static T GetItem<T>(HttpAuthenticator httpAuthenticator, string path, HttpMethod httpMethod, object content)
			where T : DatabaseEntityItem, new()
		{
			var item = GetItem(httpAuthenticator, path, httpMethod, content);
			return (T) Activator.CreateInstance(typeof(T), new object[] {item});
		}

		public static dynamic GetItem(HttpAuthenticator httpAuthenticator, string path, HttpMethod httpMethod, IDictionary<string, string> parameters)
		{
			var request = CreateHttpRequest(parameters, $"api/{path}", httpMethod, httpAuthenticator.UserCookie);
			return GetResponseFromRequest(request, httpAuthenticator);
		}

		public static dynamic GetItem(HttpAuthenticator httpAuthenticator, string path, HttpMethod httpMethod, object content)
		{
			var request = CreateHttpRequest(content, $"api/{path}", httpMethod, httpAuthenticator.UserCookie);
			return GetResponseFromRequest(request, httpAuthenticator).Result;
		}
	}
}
