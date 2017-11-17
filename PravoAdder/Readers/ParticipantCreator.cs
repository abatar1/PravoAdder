using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Repositories;
using PravoAdder.Domain;
using PravoAdder.Helpers;
using PravoAdder.Wrappers;

namespace PravoAdder.Readers
{
	public class ParticipantCreator : Creator
	{
		private VisualBlock _visualBlock;
		private readonly string _currentType;

		public ParticipantCreator(HttpAuthenticator httpAuthenticator, ApplicationArguments applicationArguments) : base(httpAuthenticator, applicationArguments)
		{
			_currentType = applicationArguments.ParticipantType;
		}

		public override ICreatable Create(Row info, Row row, DatabaseEntityItem item = null)
		{
			var type = ParticipantType.GetType(HttpAuthenticator, _currentType);

			var participant = new Participant
			{
				Type = type,
				ContactDetail = new ContactDetail(),
				VisualBlockValueLines = new List<VisualBlockParticipantLine>()
			};

			var contactProperties = typeof(ContactDetail).GetProperties();
			var participantProperties = typeof(Participant).GetProperties();

			foreach (var valuePair in row.Content)
			{
				var fieldName = info[valuePair.Key].FieldName;
				var value = valuePair.Value.Value?.Trim();			

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

				if (_currentType == ParticipantType.PersonTypeName)
				{
					foreach (var prop in participantProperties)
					{					
						var displayName = prop.LoadAttribute<DisplayNameAttribute>()?.DisplayName;
						if (displayName == null || !displayName.Equals(fieldName)) continue;

						var isRequired = prop.LoadAttribute<RequiredAttribute>();
						if (isRequired != null && string.IsNullOrEmpty(value)) return null;

						prop.SetValue(participant, value);
					}
					if (fieldName == "Company Name")
					{
						var newCompany = new Participant {Type = ParticipantType.GetCompanyType(HttpAuthenticator), Organization = value};
						var company = ParticipantsRepository.GetOrCreate<ParticipantsApi>(HttpAuthenticator, value, newCompany);
						participant.Company = company;
					}
				}
				else if (_currentType == ParticipantType.CompanyTypeName)
				{
					if (fieldName == "Organization")
					{
						participant.Organization = value;
						continue;
					}
				}

				if (_visualBlock == null)
				{
					_visualBlock = ApiRouter.VisualBlocks.GetEntityCard(HttpAuthenticator, type.Id, "Participant");
				}

				if (string.IsNullOrEmpty(value)) continue;
				participant = FillLines(participant, fieldName, value);			
			}
			return participant;
		}

		private Participant FillLines(Participant participant, string fieldName, string value)
		{
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
			return participant;
		}
	}
}
