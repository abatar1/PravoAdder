using System;
using System.Collections.Generic;
using System.ComponentModel;
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

		[DisplayName("Last Name"), Required]
		public string LastName { get; set; }

		[DisplayName("First Name"), Required]
		public string FirstName { get; set; }

		[DisplayName("Job Title")]
		public string JobTitle { get; set; }

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

		public Participant(string firstName, string lastName, ParticipantType type)
		{
			FirstName = firstName;
			LastName = lastName;
			Type = type;
		}

		public Participant(string fullname, char splitSymbol, ParticipantType type)
		{
			var splitName = fullname.Split(splitSymbol);
			if (splitName.Length != 2) throw new ArgumentException();

			FirstName = splitName[0];
			LastName = splitName[1];
			Type = type;
		}

		public Participant()
		{
		}
	}
}
