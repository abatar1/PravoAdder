﻿using System.Collections.Generic;
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
		private VisualBlockModel _visualBlock;
		private readonly string _currentType;

		public ParticipantCreator(HttpAuthenticator httpAuthenticator, Settings settings) : base(httpAuthenticator, settings)
		{
			_currentType = settings.ParticipantType;
		}

		public override ICreatable Create(Table table, Row row, DatabaseEntityItem item = null)
		{
			var type = ParticipantType.GetType(HttpAuthenticator, _currentType);

			var participant = new Participant
			{
				Type = type,
				ContactDetail = new ContactDetail(),
				VisualBlockValueLines = new List<VisualBlockLine>()
			};

			var contactProperties = typeof(ContactDetail).GetProperties();
			var participantProperties = typeof(Participant).GetProperties();

			foreach (var valuePair in row.Content)
			{
				var fieldName = table.Header[valuePair.Key].FieldName;
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
						var displayName = prop.GetAttribute<DisplayNameAttribute>()?.DisplayName;
						if (displayName == null || !displayName.Equals(fieldName)) continue;

						var isRequired = prop.GetAttribute<Domain.RequiredAttribute>();
						if (isRequired != null && string.IsNullOrEmpty(value)) return null;

						prop.SetValue(participant, value);
					}
					if (fieldName == "Company")
					{
						var newCompany = new Participant {Type = ParticipantType.GetCompanyType(HttpAuthenticator), Organization = value};
						var company = ParticipantsRepository.GetOrCreate(HttpAuthenticator, value, newCompany);
						participant.Company = company;
					}
				}
				else if (_currentType == ParticipantType.CompanyTypeName)
				{
					if (fieldName == "Company")
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

				var newField = new VisualBlockField
				{
					Value = FieldBuilder.CreateFieldValueFromData(HttpAuthenticator, field, value),
					VisualBlockProjectFieldId = field.Id
				};

				var lineIndex = participant.VisualBlockValueLines.FindIndex(l => l.BlockLineId == line.Id);
				if (lineIndex != -1)
				{
					if (participant.VisualBlockValueLines[lineIndex].Values.Count == 0)
					{
						participant.VisualBlockValueLines[lineIndex].Values = new List<VisualBlockField> { newField };
					}
					else
					{
						participant.VisualBlockValueLines[lineIndex].Values.Add(newField);
					}
				}
				else
				{
					var newLine = new VisualBlockLine
					{
						BlockLineId = line.Id,
						Order = 0,
						Values = new List<VisualBlockField> {newField}
					};
					participant.VisualBlockValueLines.Add(newLine);
				}
			}
			return participant;
		}
	}
}
