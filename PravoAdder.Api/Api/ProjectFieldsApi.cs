using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api.Api
{
	public class ProjectFieldsApi
	{
		public ProjectField CreateProjectField(HttpAuthenticator httpAuthenticator, ProjectField projectField)
		{
			return ApiHelper.GetItem<ProjectField>(httpAuthenticator, "ProjectFields/CreateProjectField", HttpMethod.Post, projectField);
		}
	}
}
