using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ParticipantsApi : IApi<Participant>
	{
		public List<Participant> GetMany(HttpAuthenticator httpAuthenticator, string optional = null)
		{
			return ApiHelper.GetItems<Participant>(httpAuthenticator, "ParticipantsSuggest/GetParticipants", HttpMethod.Post);
		}

		public Participant Get(HttpAuthenticator httpAuthenticator, string participantId)
		{
			var parameters = ApiHelper.CreateParameters(("participantId", participantId));
			return ApiHelper.GetItem<Participant>(httpAuthenticator, "participants/GetParticipant", HttpMethod.Get, parameters);
		}
		
		public Participant Create(HttpAuthenticator httpAuthenticator, Participant participant)
		{
			return ApiHelper.GetItem<Participant>(httpAuthenticator, "participants/PutParticipant", HttpMethod.Put, participant);
		}

		public void Delete(HttpAuthenticator httpAuthenticator, string participantId)
		{
			ApiHelper.SendItem(httpAuthenticator, $"participants/DeleteParticipant/{participantId}", HttpMethod.Delete,
				new Dictionary<string, string>());
		}
	}
}
