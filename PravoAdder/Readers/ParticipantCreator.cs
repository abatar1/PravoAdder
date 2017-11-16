using System;
using System.Collections.Generic;
using System.Linq;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Repositories;
using PravoAdder.Domain;
using PravoAdder.Wrappers;

namespace PravoAdder.Readers
{
	public class ParticipantCreator : ICreator
	{
		private VisualBlock _visualBlock;
		private readonly List<ParticipantType> _participantTypes;
		private readonly string _currentType;

		public static readonly string Person = "Person";
		public static readonly string Company = "Company";

		public ParticipantCreator(HttpAuthenticator authenticator, string typeName)
		{
			if (typeName != Person && typeName != Company) throw new ArgumentException("Wrong participant type name");

			_currentType = typeName;
			HttpAuthenticator = authenticator;
			_participantTypes = ApiRouter.Bootstrap.GetParticipantTypes(HttpAuthenticator);
		}

		public HttpAuthenticator HttpAuthenticator { get; }

		public ICreatable Create(Row info, Row row, DatabaseEntityItem item = null)
		{
			var type = _participantTypes.First(p => p.Name == _currentType);

			var participant = new Participant
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
				if (string.IsNullOrEmpty(value)) continue;

				var contactProperty = contactProperties.FirstOrDefault(p => p.Name == fieldName);
				if (contactProperty != null)
				{
					contactProperty.SetValue(participant.ContactDetail, value);
					continue;
				}

				if (fieldName == "INN")
				{
					participant.Inn = value;
				}

				if (_currentType == Person)
				{
					if (fieldName == "First Name")
					{
						participant.FirstName = value;
						continue;
					}
					if (fieldName == "Last Name")
					{
						participant.LastName = value;
						continue;
					}
					if (fieldName == "Company Name")
					{
						var newCompany = new Participant {Type = _participantTypes.First(p => p.Name == Company), Organization = value};
						var company = ParticipantsRepository.GetOrCreate<ParticipantsApi>(HttpAuthenticator, value, newCompany);
						participant.Company = company;
					}
				}
				else if (_currentType == Company)
				{
					if (fieldName == "Organization Name")
					{
						participant.Organization = value;
						continue;
					}
				}

				if (_visualBlock == null)
				{
					var participantTypeId = _participantTypes.First(p => p.TypeName == _currentType).Id;
					_visualBlock = ApiRouter.VisualBlocks.GetEntityCard(HttpAuthenticator, participantTypeId, "Participant");
				}
				foreach (var line in _visualBlock.Lines)
				{
					var field = line.Fields.FirstOrDefault(f => f.ProjectField.Name == fieldName);
					if (field == null) continue;

					var newField = new VisualBlockParticipantField
					{
						Value = FieldBuilder.CreateFieldValueFromData(HttpAuthenticator, field, value),
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
			return participant;
		}
	}
}
