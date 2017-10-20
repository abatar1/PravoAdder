using System.Collections.Generic;
using Newtonsoft.Json;

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
		public string Organization { get; set; }
		public string CreationDate { get; set; }

		[JsonIgnore]
		public string FullName
		{
			get
			{
				if (!string.IsNullOrEmpty(FirstName) || !string.IsNullOrEmpty(MiddleName) || !string.IsNullOrEmpty(LastName))
				{
					return $"{FirstName} {MiddleName} {LastName}";
				}
				if (!string.IsNullOrEmpty(Organization))
				{
					return Organization;
				}			
				return string.Empty;
			}
		}
		public List<VisualBlockParticipantLine> VisualBlockValueLines { get; set; }
	}
}
