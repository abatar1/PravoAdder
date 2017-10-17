using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class ExtendentParticipant
	{
		public ParticipantType Type { get; set; }
		public Participant Company { get; set; }
		public ContactDetail ContactDetail { get; set; }
		public string LastName { get; set; }
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public List<VisualBlockParticipantLine> VisualBlockValueLines { get; set; }
	}
}
