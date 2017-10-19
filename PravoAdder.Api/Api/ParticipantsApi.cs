using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

<<<<<<< HEAD
		public List<Participant> GetParticipants(HttpAuthenticator httpAuthenticator)
=======
		public bool PutParticipant(HttpAuthenticator httpAuthenticator, ExtendentParticipant participant)
		{
			return ApiHelper.TrySendAsync(httpAuthenticator, "participants/PutParticipant", HttpMethod.Put, participant).Result;
		}

		public IList<Participant> GetParticipants(HttpAuthenticator httpAuthenticator)
>>>>>>> e06ccc4eb4c20f5b0a884c8c73b5e112fbac295a
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

		public bool PutParticipant(HttpAuthenticator httpAuthenticator, ExtendentParticipant participant)
		{
			return ApiHelper.TrySendAsync(httpAuthenticator, "participants/PutParticipant", HttpMethod.Put, participant).Result;
		}

		public void DeleteParticipant(HttpAuthenticator httpAuthenticator, string participantId)
		{
			ApiHelper.GetItem(httpAuthenticator, $"participants/DeleteParticipant/{participantId}", HttpMethod.Delete,
				new Dictionary<string, string>());
		}
	}
}
