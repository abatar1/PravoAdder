using System;
using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class ExtendentParticipant
	{
		public ExtendentParticipant(ParticipantType participantType, Participant company, ContactDetail contactDetail,
			string lastName, string firstName, string middleName, List<VisualBlockLine> visualBlockValueLines)
		{
			ParticipantType = participantType;
			Company = company;
			ContactDetail = contactDetail;
			LastName = lastName;
			FirstName = firstName;
			MiddleName = middleName;
			VisualBlockValueLines = visualBlockValueLines;
		}

		public ParticipantType ParticipantType { get; }
		public Participant Company { get; }
		public ContactDetail ContactDetail { get; }
		public string LastName { get; }
		public string FirstName { get; }
		public string MiddleName { get; }
		public List<VisualBlockLine> VisualBlockValueLines { get; }
	}
}
