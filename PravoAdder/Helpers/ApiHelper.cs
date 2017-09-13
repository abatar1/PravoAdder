using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Domain;

namespace PravoAdder.Helpers
{
	public class ApiHelper
	{
		private static dynamic GetMessageFromRequest(HttpRequestMessage request, HttpAuthenticator httpAuthenticator)
		{
			var response = httpAuthenticator.Client.SendAsync(request).Result;
			response.EnsureSuccessStatusCode();

			return !response.IsSuccessStatusCode ? null : HttpHelper.GetMessageFromResponceAsync(response).Result;
		}

		public static IList<T> SendWithManyPagesRequest<T>(HttpAuthenticator httpAuthenticator, string path, HttpMethod httpMethod, 
			IDictionary<string, string> additionalContent = null)
		{
			var count = 1;
			var resultContainer = new List<T>();
			do
			{
				dynamic content = new ExpandoObject();			
				content.Add("PageSize", Api.Api.PageSize);
				content.Add("Page", count);
				if (additionalContent != null)
				{
					foreach (var pair in additionalContent)
					{
						content.Add(pair.Key, pair.Value);
					}
				}

				var request = HttpHelper.CreateRequest((object) content, $"api/{path}", httpMethod,
					httpAuthenticator.UserCookie);

				var responseMessage = GetMessageFromRequest(request, httpAuthenticator);
				if (responseMessage == null) return null;
				if (!responseMessage.NextPageExists) break;

				var newItems = new List<dynamic>(responseMessage.Result)
					.Select(r => (T) Activator.CreateInstance(typeof(T), new object[] { r }));
				resultContainer.AddRange(newItems);

				count += 1;
			} while (true);

			return resultContainer.Count == 0 ? null : resultContainer;
		}

		public static T SendDatabaseEntityItem<T>(dynamic content, string path, HttpMethod httpMethod, HttpAuthenticator httpAuthenticator) 
			where T : DatabaseEntityItem, new()
		{		
			var request = HttpHelper.CreateRequest((object) content, $"api/{path}", httpMethod,
				httpAuthenticator.UserCookie);

			var responseMessage = GetMessageFromRequest(request, httpAuthenticator).Result;
			if (responseMessage == null) return null;

			return (T) Activator.CreateInstance(typeof(T), new object[] { responseMessage.Name.ToString(),
				responseMessage.Id.ToString() });
		}

		public static async Task<bool> TrySendAsync(HttpAuthenticator httpAuthenticator, dynamic content, string path, HttpMethod httpMethod)
		{
			var request = HttpHelper.CreateRequest((object) content, $"api/{path}", httpMethod, httpAuthenticator.UserCookie);

			var response = await httpAuthenticator.Client.SendAsync(request);

			return response != null && response.IsSuccessStatusCode;
		}

		public static dynamic SendDynamicItem(HttpAuthenticator httpAuthenticator, string path, HttpMethod httpMethod, IDictionary<string, string> parameters)
		{
			var request = HttpHelper.CreateRequest(parameters, $"api/{path}", httpMethod, httpAuthenticator.UserCookie);

			return GetMessageFromRequest(request, httpAuthenticator).Result;
		}
	}
}
