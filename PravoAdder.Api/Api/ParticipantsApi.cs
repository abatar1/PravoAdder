using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ParticipantsApi
	{
		public Participant PutParticipant(HttpAuthenticator httpAuthenticator, string organizationName, string vad)
		{
			var content = new
			{
				Organization = organizationName,
				Type = GetParticipantTypes(httpAuthenticator).First(p => p.Name == "Организация"),
				INN = vad
			};

			return ApiHelper.GetItem<Participant>(httpAuthenticator, "participants/PutParticipant", HttpMethod.Put, content);
		}

		public bool PutParticipant(HttpAuthenticator httpAuthenticator, ExtendentParticipant participant)
		{
			return ApiHelper.TrySendAsync(httpAuthenticator, "participants/PutParticipant", HttpMethod.Put, participant).Result;
		}

		public IList<Participant> GetParticipants(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.GetItems<Participant>(httpAuthenticator, "ParticipantsSuggest/GetParticipants", HttpMethod.Post);
		}

		public List<ParticipantType> GetParticipantTypes(HttpAuthenticator httpAuthenticator)
		{
			IEnumerable<dynamic> content = ApiHelper
				.GetItem(httpAuthenticator, "bootstrap/GetBootstrap", HttpMethod.Get, new Dictionary<string, string>())
				["CaseMap.Modules.Main"]["CaseMap.Modules.Participants"]["ParticipantTypes"];
			return content.Select(o => new ParticipantType(o)).ToList();
		}
	}
}
