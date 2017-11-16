using System.Collections.Generic;
using Newtonsoft.Json;

namespace PravoAdder.Api.Domain
{
	public class Participant : DatabaseEntityItem, ICreatable
	{
		public ParticipantType Type { get; set; }

		public Participant Company { get; set; }

		public ContactDetail ContactDetail { get; set; }

		public string TypeName { get; set; }

		[JsonProperty("INN")]
		public string Inn { get; set; }

		public string LastName { get; set; }

		public string FirstName { get; set; }

		public string MiddleName { get; set; }

		public string Organization { get; set; }

		public string CreationDate { get; set; }

		public string IncludeInProjectId { get; set; }

		public List<VisualBlockParticipantLine> VisualBlockValueLines { get; set; }
		public VisualBlock VisualBlock { get; set; }

		[JsonIgnore]
		public string FullName
		{
			get
			{
				if (!string.IsNullOrEmpty(FirstName) || !string.IsNullOrEmpty(LastName))
				{
					var midName = MiddleName;
					if (!string.IsNullOrEmpty(midName)) midName = $" {MiddleName}";
					return $"{FirstName}{midName} {LastName}";
				}
				if (!string.IsNullOrEmpty(Organization))
				{
					return Organization;
				}			
				return string.Empty;
			}
		}

		public override string ToString() => FullName;
	}
}
