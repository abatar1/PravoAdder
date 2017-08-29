using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using PravoAdder.Helpers;

namespace PravoAdder.DatabaseEnviroment
{
	public class DatabaseGetter
	{
		private readonly HttpAuthenticator _httpAuthenticator;

		public DatabaseGetter(HttpAuthenticator authenticator)
		{
			_httpAuthenticator = authenticator;
		}

		private IEnumerable<dynamic> GetMessageFromRequest(HttpRequestMessage request)
		{
			var response = _httpAuthenticator.Client.SendAsync(request).Result;
			response.EnsureSuccessStatusCode();

			if (!response.IsSuccessStatusCode) yield break;

			var messages = HttpHelper.GetMessageFromResponceAsync(response).Result;
			foreach (var message in messages.Result)
			{
				yield return message;
			}
		}

		private IEnumerable<dynamic> GetJsonPages(object content, string uri, HttpMethod httpMethod)
		{
			var request = HttpHelper.CreateJsonRequest(content, $"api/{uri}", httpMethod, _httpAuthenticator.UserCookie);

			return GetMessageFromRequest(request).ToList();
		}

		private IEnumerable<dynamic> GetPages(IDictionary<string, string> parameters, string uri, HttpMethod httpMethod)
		{
			var request = HttpHelper.CreateRequest(parameters, $"api/{uri}", httpMethod, _httpAuthenticator.UserCookie);

			return GetMessageFromRequest(request).ToList();
		}

		private dynamic GetSimpleJsonPage(string name, string uri, HttpMethod httpMethod, int pageSize = int.MaxValue)
		{
			var content = new
			{
				PageSize = pageSize,
				Page = 1
			};
			return GetJsonPages(content, uri, httpMethod)
				.Single(p => p.Name == name);
		}

		private IEnumerable<dynamic> GetSimpleJsonPages(string uri, HttpMethod httpMethod, int pageSize = int.MaxValue)
		{
			var content = new
			{
				PageSize = pageSize,
				Page = 1
			};
			foreach (var page in GetJsonPages(content, uri, httpMethod))
			{
				yield return page;
			}
		}

		public dynamic GetProjectGroup(string projectName, int pageSize = int.MaxValue)
		{
			var content = new
			{
				PageSize = pageSize,
				Page = 1
			};

			var pages = GetJsonPages(content, "ProjectGroups/PostProjectGroups", HttpMethod.Post);

			foreach (var page in pages)
			{
				var name = (string) page?["Name"];
				if (name == projectName)
					return new
					{
						Name = name,
						Id = (string) page?["Id"]
					};
			}
			return null;
		}

		public dynamic GetProject(string projectName, string projectGroupid, string folderName, int pageSize = int.MaxValue)
		{
			var content = new
			{
				PageSize = pageSize,
				Page = 1,
				FolderId = GetProjectFolder(folderName).Id
			};

			var projectGroup = GetJsonPages(content, "Projects/GetGroupedProjects", HttpMethod.Post)
				.FirstOrDefault(pf => pf["ProjectGroupResponse"]["Id"] == projectGroupid);

			var projects = projectGroup?["Projects"];
			if (projects == null) return null;

			foreach (var project in projects)
			{
				var name = (string) project?["Name"];
				if (name == projectName)
				{
					return new
					{
						Name = (string) project?["Name"],
						Id = (string) project?["Id"]
					};
				}
			}
			return null;
		}

		public dynamic GetProjectType(string projectTypeName)
		{
			return GetSimpleJsonPage(projectTypeName, "ProjectTypes/GetProjectTypes", HttpMethod.Post);
		}

		public dynamic GetResponsible(string responsibleName)
		{
			return GetSimpleJsonPage(responsibleName.Replace(".", ""), "CompanyUsersSuggest", HttpMethod.Post);
		}

		public dynamic GetProjectFolder(string folderName)
		{
			return GetSimpleJsonPage(folderName, "ProjectFolders/GetProjectFoldersForEdit", HttpMethod.Post);
		}

		public dynamic GetParticipant(string participantName)
		{
			return GetSimpleJsonPage(participantName, "ParticipantsSuggest/GetParticipants", HttpMethod.Post);
		}

		public dynamic GetCalculationFormulas(string formulaName)
		{
			return GetSimpleJsonPage(formulaName, "CalculationFormulasSuggest/GetCalculationFormulas", HttpMethod.Post);
		}

		public IList<dynamic> GetDictionary(string dictionaryName)
		{
			return GetSimpleJsonPages($"dictionary/{dictionaryName}/getdictionaryitems", HttpMethod.Post).ToList();
		}

		public IList<dynamic> GetParticipants()
		{
			return GetSimpleJsonPages("ParticipantsSuggest/GetParticipants", HttpMethod.Post).ToList();
		}

		public dynamic GetVisualBlocks(string projectTypeId)
		{
			var parameters = new Dictionary<string, string> {["projectTypeId"] = projectTypeId};
			var pages = GetPages(parameters, "ProjectTypes/GetProjectType", HttpMethod.Get)
				.First(block => block.Name == "VisualBlocks").Value;

			return pages;
		}
	}
}