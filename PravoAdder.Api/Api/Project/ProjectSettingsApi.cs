using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api.Api
{
	public class ProjectSettingsApi
	{
		public void Save(HttpAuthenticator authenticator, ProjectSettings projectSettings)
		{
			ApiHelper.SendItem(authenticator, "ProjectSettings/SaveSettings", HttpMethod.Put, projectSettings);
		}

		public ProjectSettings Get(HttpAuthenticator authenticator, string projectId)
		{
			var parameters = ApiHelper.CreateParameters(("projectId", projectId));
			return ApiHelper.GetItem<ProjectSettings>(authenticator, "ProjectSettings/GetSettings", HttpMethod.Get, parameters);
		}
	}
}
