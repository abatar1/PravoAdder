using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Domain;
using PravoAdder.Helpers;

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

			return ApiHelper.SendDatabaseEntityItem<Participant>(content, "participants/PutParticipant", HttpMethod.Put,
				httpAuthenticator);
		}

		public IList<Participant> GetParticipants(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.SendWithManyPagesRequest<Participant>(httpAuthenticator, "ParticipantsSuggest/GetParticipants", HttpMethod.Post);
		}
	}
}
