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
		public string TypeId { get; set; }

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

		public List<VisualBlockLine> VisualBlockValueLines { get; set; }
		public VisualBlockModel VisualBlock { get; set; }

		[JsonIgnore]
		public override string DisplayName
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
				return Name;
			}
		}		

		public override string ToString() => DisplayName;

		public Participant(string firstName, string lastName, ParticipantType type)
		{
			FirstName = firstName;
			LastName = lastName;
			Type = type;
		}

		public Participant(HttpAuthenticator authenticator, string fullname, char splitSymbol)
		{
			var splitName = fullname.Split(splitSymbol);

			if (splitName.Length == 2)
			{
				FirstName = splitName[0];
				LastName = splitName[1];
				Type = ParticipantType.GetPersonType(authenticator);
			}
			else if (splitName.Length == 3)
			{
				FirstName = splitName[0];
				MiddleName = splitName[1];
				LastName = splitName[2];
				Type = ParticipantType.GetPersonType(authenticator);
			}
			else
			{
				Organization = fullname;
				Type = ParticipantType.GetCompanyType(authenticator);
			}
		}

		public Participant()
		{
		}
	}
}
