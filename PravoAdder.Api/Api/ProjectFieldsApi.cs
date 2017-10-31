﻿using System.Collections.Generic;
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

		public List<ProjectField> Get(HttpAuthenticator httpAuthenticator, string name)
		{
			var content = new
			{
				Name = name
			};
			return ApiHelper.GetItem<List<ProjectField>>(httpAuthenticator, "ProjectFieldSuggest/GetProjectFields", HttpMethod.Post, content);
		}
	}
}
