using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ParticipantsApi
	{
		public Participant PutParticipant(HttpAuthenticator httpAuthenticator, string name, string projectId = null)
		{
			var content = new
			{
				Organization = name,
				Type = GetParticipantTypes(httpAuthenticator).First(p => p.Name == "Организация"),
				IncludeInProjectId = projectId
			};

			return ApiHelper.GetItem<Participant>(httpAuthenticator, "participants/PutParticipant", HttpMethod.Put, content);
		}

		public List<Participant> GetParticipants(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.GetItems<Participant>(httpAuthenticator, "ParticipantsSuggest/GetParticipants", HttpMethod.Post);
		}

		public DetailedParticipant GetParticipant(HttpAuthenticator httpAuthenticator, string participantId)
		{
			var parameters = ApiHelper.CreateParameters(("participantId", participantId));
			return ApiHelper.GetItem<DetailedParticipant>(httpAuthenticator, "participants/GetParticipant", HttpMethod.Get, parameters);
		}

		public VisualBlock GetVisualBlock(HttpAuthenticator httpAuthenticator, string participantId)
		{
			var parameters = ApiHelper.CreateParameters(("participantId", participantId));
			return ApiHelper.GetItem<VisualBlockWrapper>(httpAuthenticator, "participants/GetParticipant", HttpMethod.Get, parameters).VisualBlock;
		}

		public List<ParticipantType> GetParticipantTypes(HttpAuthenticator httpAuthenticator)
		{
			var bootstrap = ApiRouter.Bootstrap.GetBootstrap(httpAuthenticator);
			IEnumerable<dynamic> participantTypes = bootstrap["CaseMap.Modules.Main"]["CaseMap.Modules.Participants"]["ParticipantTypes"];
			return participantTypes.Select(o => JsonConvert.DeserializeObject<ParticipantType>(0.ToString())).ToList();
		}

		public Participant PutParticipant(HttpAuthenticator httpAuthenticator, DetailedParticipant participant)
		{
			return ApiHelper.GetItem<Participant>(httpAuthenticator, "participants/PutParticipant", HttpMethod.Put, participant);
		}

		public void DeleteParticipant(HttpAuthenticator httpAuthenticator, string participantId)
		{
			ApiHelper.SendItem(httpAuthenticator, $"participants/DeleteParticipant/{participantId}", HttpMethod.Delete,
				new Dictionary<string, string>());
		}
	}
}
