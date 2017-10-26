using System.Collections.Generic;
using Newtonsoft.Json;

namespace PravoAdder.Api.Domain
{
	public class DetailedParticipant
	{
		public string Id { get; set; }
		public ParticipantType Type { get; set; }
		public Participant Company { get; set; }
		public ContactDetail ContactDetail { get; set; }
		public string INN { get; set; }
		public string LastName { get; set; }
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string Organization { get; set; }
		public string CreationDate { get; set; }
		public string IncludeInProjectId { get; set; }
		public List<VisualBlockParticipantLine> VisualBlockValueLines { get; set; }

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

		public static explicit operator Participant(DetailedParticipant other)
		{
			return new Participant { Name = other.FullName, Id = other.Id };
		}

		public Participant ToParticipant() => new Participant { Name = FullName, Id = Id };

		public override string ToString() => FullName;
	}
}
