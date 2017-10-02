using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ParticipantsApi
	{
		public Participant PutParticipant(HttpAuthenticator httpAuthenticator, string organizationName, string projectId = null)
		{
			var content = new
			{
				IncludeInProjectId = projectId,
				Organization = organizationName,
				Type = new
				{
					Id = "3f2e9588-97c0-e611-80b8-0cc47a7d3f5d",
					action = "add",
					Name = "Company",
					NameEn = "company"
				}
			};

			return ApiHelper.SendDatabaseEntityItem<Participant>(content, "participants/PutParticipant", HttpMethod.Put,
				httpAuthenticator);
		}

		public IList<Participant> GetParticipants(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.SendWithManyPagesRequest<Participant>(httpAuthenticator, "ParticipantsSuggest/GetParticipants", HttpMethod.Post);
		}
	}
}
