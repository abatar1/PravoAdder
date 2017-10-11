using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ParticipantsApi
	{
		public Participant PutParticipant(HttpAuthenticator httpAuthenticator, string organizationName)
		{
			var content = new
			{
				Organization = organizationName,
				Type = new
				{
					Id = "92ffb67f-fac0-e611-8b3a-902b343a9588",
					action = "add",
					Name = "Организация",
					NameEn = "company"
				}
			};

			return ApiHelper.GetItem<Participant>(content, "participants/PutParticipant", HttpMethod.Put,
				httpAuthenticator);
		}

		public IList<Participant> GetParticipants(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.GetItems<Participant>(httpAuthenticator, "ParticipantsSuggest/GetParticipants", HttpMethod.Post);
		}

		public List<ParticipantType> GetParticipantTypes(HttpAuthenticator httpAuthenticator)
		{
			return (List<ParticipantType>) ApiHelper
				.GetItem(httpAuthenticator, "bootstrap/GetBootstrap", HttpMethod.Post, null).CaseMap.Modules.Participants
				.ParticipantTypes;
		}
	}
}
