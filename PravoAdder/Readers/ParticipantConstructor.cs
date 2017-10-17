using System.Collections.Generic;
using System.Linq;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Domain;

namespace PravoAdder.Readers
{
	public class ParticipantConstructor
	{
		private readonly HttpAuthenticator _authenticator;
		private readonly VisualBlock _visualBlock;

		public ParticipantConstructor(HttpAuthenticator authenticator)
		{
			_authenticator = authenticator;
			var participantTypeId = ApiRouter.Participants.GetParticipantTypes(_authenticator)
				.First(p => p.TypeName == "Person").Id;
			_visualBlock = ApiRouter.ProjectTypes.GetEntityCardVisualBlock(_authenticator, participantTypeId, "Participant");
		}

		public ExtendentParticipant Create(Row info, Row row)
		{
			var type = ApiRouter.Participants.GetParticipantTypes(_authenticator).First(p => p.Name == "Person");
			var participant = new ExtendentParticipant
			{
				Type = type,
				ContactDetail = new ContactDetail(),
				VisualBlockValueLines = new List<VisualBlockParticipantLine>()
			};

			var contactProperties = typeof(ContactDetail).GetProperties();

			foreach (var valuePair in row.Content)
			{
				var fieldName = info[valuePair.Key].FieldName;
				var value = valuePair.Value.Value?.Trim();

				var contactProperty = contactProperties.FirstOrDefault(p => p.Name == fieldName);
				if (contactProperty != null)
				{
					contactProperty.SetValue(participant.ContactDetail, value);
				}
				else if (fieldName == "First name")
				{
					participant.FirstName = value;
				}
				else if (fieldName == "Last name")
				{
					participant.LastName = value;
				}
				else
				{
					foreach (var line in _visualBlock.Lines)
					{
						var field = line.Fields.FirstOrDefault(f => f.ProjectField.Name == fieldName);
						if (field == null) continue;

						var newField = new VisualBlockParticipantField
						{
							Value = value,
							VisualBlockProjectFieldId = field.Id
						};

						var lineIndex = participant.VisualBlockValueLines.FindIndex(l => l.BlockLineId == line.Id);
						if (lineIndex != -1)
						{
							if (participant.VisualBlockValueLines[lineIndex].Values.Count == 0)
							{
								participant.VisualBlockValueLines[lineIndex].Values = new List<VisualBlockParticipantField> { newField };
							}
							else
							{
								participant.VisualBlockValueLines[lineIndex].Values.Add(newField);
							}								
						}
						else
						{
							var newLine = new VisualBlockParticipantLine
							{
								BlockLineId = line.Id,
								Order = 0,
								Values = new List<VisualBlockParticipantField> { newField }
							};
							participant.VisualBlockValueLines.Add(newLine);
						}
					}
				}
			}

			return participant;
		}
	}
}
