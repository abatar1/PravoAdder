using System.Collections.Generic;
using System.Linq;
using PravoAdder.Api.Domain;

namespace PravoAdder.Api.Repositories
{
	public class ParticipantsRepository : TemplateRepository<Participant>
	{
		public static IEnumerable<Participant> GetManyDetailed(HttpAuthenticator authenticator)
		{
			return GetMany<ParticipantsApi>(authenticator).Select(p => GetDetailed<ParticipantsApi>(authenticator, p.FullName));
		}
	}
}
