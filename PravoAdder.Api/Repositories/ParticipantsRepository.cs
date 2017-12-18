using System.Collections.Generic;
using System.Linq;
using PravoAdder.Api.Domain;

namespace PravoAdder.Api.Repositories
{
	public class ParticipantsRepository : TemplateRepository<Participant, ParticipantsApi>
	{
		public static IEnumerable<Participant> GetManyDetailed(HttpAuthenticator authenticator)
		{
			return GetMany(authenticator).Select(p => GetDetailed(authenticator, p.DisplayName));
		}
	}
}
