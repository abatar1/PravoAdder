using System.Collections.Generic;
<<<<<<< HEAD
using Newtonsoft.Json;
=======
>>>>>>> e06ccc4eb4c20f5b0a884c8c73b5e112fbac295a

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
<<<<<<< HEAD
		public string Organization { get; set; }

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
=======
>>>>>>> e06ccc4eb4c20f5b0a884c8c73b5e112fbac295a
		public List<VisualBlockParticipantLine> VisualBlockValueLines { get; set; }
	}
}
