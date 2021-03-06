﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace PravoAdder.Api.Helpers
{
	internal class ApiHelper
	{
		private static dynamic GetResponseFromRequest(HttpRequestMessage request, HttpAuthenticator httpAuthenticator)
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
			HttpRequestMessage request;
			if (content is IDictionary<string, string> dictionary)
			{
				var parametersString = string.Empty;
				if (dictionary.Count > 0)
				{
					var parametersBuilder = new StringBuilder();
					parametersBuilder.Append("?");
					foreach (var parameter in dictionary)
					{
						parametersBuilder.Append($"{parameter.Key}={parameter.Value}&");
					}
					parametersString = parametersBuilder.ToString().Remove(parametersBuilder.Length - 1);
				}

				request = new HttpRequestMessage(method, $"{requestUri}{parametersString}");
			}
			else
			{
				request = new HttpRequestMessage(method, requestUri);
				if (content != null)
				{
					var serializedContent = JsonConvert.SerializeObject(content, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
					request.Content = new StringContent(serializedContent, Encoding.UTF8, "application/json");
				}
			}

			request.Headers.Add("Cookie", cookie.ToString());

			return request;
		}

		public static List<T> GetItems<T>(HttpAuthenticator httpAuthenticator, string path, HttpMethod httpMethod, IDictionary<string, string> parameters = null)
		{
			var count = 1;
			var resultContainer = new List<T>();
			do
			{
				var content = new ExpandoObject() as IDictionary<string, object>;
				content.Add("PageSize", ApiRouter.PageSize);
				content.Add("Page", count);
				if (parameters != null)
				{
					foreach (var pair in parameters)
					{
						content.Add(pair.Key, pair.Value);
					}
				}
				var request = CreateHttpRequest(content, $"api/{path}", httpMethod,
					httpAuthenticator.UserCookie);

				var responseMessage = GetResponseFromRequest(request, httpAuthenticator);
				if (responseMessage == null) throw new HttpRequestException();

				var newItems = new List<object>(responseMessage.Result)
					.Select(r => (T) JsonConvert.DeserializeObject(r.ToString(), typeof(T)));
				resultContainer.AddRange(newItems);

				count += 1;
				if (!(bool) responseMessage.NextPageExists) break;

			} while (true);

			return resultContainer;
		}

		public static T GetItem<T>(HttpAuthenticator httpAuthenticator, string path, HttpMethod httpMethod, object content)
			where T : new()
		{
			var item = GetItem(httpAuthenticator, path, httpMethod, content);

			if (item == null) return default(T);
			if (!(bool)item.IsSuccess) return default(T);

			var value = item.Result.ToString();

			var converter = TypeDescriptor.GetConverter(typeof(T));
			if (converter.IsValid(value))
			{
				return converter.ConvertFromString(value);
			}

			return (T)JsonConvert.DeserializeObject(value, typeof(T));
		}

		public static dynamic GetItem(HttpAuthenticator httpAuthenticator, string path, HttpMethod httpMethod, object content)
		{
			var request = CreateHttpRequest(content, $"api/{path}", httpMethod, httpAuthenticator.UserCookie);
			return GetResponseFromRequest(request, httpAuthenticator);
		}

		public static void SendItem(HttpAuthenticator httpAuthenticator, string path, HttpMethod httpMethod, object content)
		{
			var request = CreateHttpRequest(content, $"api/{path}", httpMethod, httpAuthenticator.UserCookie);
			httpAuthenticator.Client.SendAsync(request);
		}

		public static Dictionary<string, string> CreateParameters(params (string, string)[] pairs)
		{
			return pairs.ToDictionary(pair => pair.Item1, pair => pair.Item2);
		}

		public static async Task<T> SendFileAsync<T>(HttpAuthenticator httpAuthenticator, string path, HttpMethod httpMethod, FileInfo file)
		{
			var byteContent = File.ReadAllBytes(file.FullName);
			var bytesContent = new ByteArrayContent(byteContent);

			using (var formData = new MultipartFormDataContent())
			{
				formData.Add(bytesContent, file.Name, file.Name);
				var response = await httpAuthenticator.Client.PostAsync($"api/{path}", formData);
				if (!response.IsSuccessStatusCode) return default(T);

				var responseResult = ReadFromResponce(response).Result.ToString();
				return (T) JsonConvert.DeserializeObject(responseResult, typeof(T));
			}
		}
	}
}
