using System;
using System.Collections.Generic;
using System.Linq;
using Fclp.Internals.Extensions;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Repositories;
using PravoAdder.Domain;
using PravoAdder.Helpers;

namespace PravoAdder.Processors
{
	public class ParticipantProcessor
	{
		private static Participant EditParticipant(Table table, Participant participant, HttpAuthenticator authenticator, Row row, string searchKey)
		{
			if (participant.VisualBlockValueLines == null) participant.VisualBlockValueLines = new List<VisualBlockLine>();

			var blockLines = ParticipantsRepository.Get(authenticator, participant.Id).VisualBlock.Lines;

			var fieldValue = table.GetValue(row, searchKey)?.Trim();
			if (string.IsNullOrEmpty(fieldValue)) return null;

			var editingLine = blockLines.FirstOrDefault(line => line.Fields.Any(field =>
			{
				var isRequiredFieldName = field.ProjectField.Name.Contains(searchKey);
				var doesAlreadyContains = participant.VisualBlockValueLines.FirstOrDefault(pl => pl.BlockLineId == line.Id) != null;
				return isRequiredFieldName && !doesAlreadyContains;
			}));
			if (editingLine == null) return null;

			var addingLine = new VisualBlockLine
			{
				BlockLineId = editingLine.Id,
				Values = editingLine.Fields
					.Select(f =>
					{
						var newField = new VisualBlockField { VisualBlockProjectFieldId = f.Id };
						if (f.ProjectField.Name.Contains(searchKey)) newField.Value = fieldValue;
						return newField;
					})
					.ToList()
			};
			participant.VisualBlockValueLines.Add(addingLine);
			if (string.IsNullOrEmpty(participant.LastName)) participant.LastName = "LastNameTemplate";
			return participant;
		}

		public Func<EngineMessage, EngineMessage> Create = message =>
		{
			var newParticipant = message.GetCreatable<Participant>();
			if (newParticipant == null) return null;
			var existedParticipant =
				ParticipantsRepository.GetOrCreate(message.Authenticator, newParticipant.DisplayName, newParticipant);
			if (existedParticipant!= null && !existedParticipant.IsNew) return null;

			message.Item = existedParticipant;
			return message;
		};

		public Func<EngineMessage, EngineMessage> Distinct = message =>
		{
			var response = ApiRouter.Participants.GetMany(message.Authenticator);
			var counter = message.Counter.CloneJson();
			response.GroupBy(p => p.Name).ForEach(g =>
			{
				if (g.Count() > 1)
				{
					var gCount = g.Count() - 1;
					g.Skip(1).Select((p, c) => new { Participant = p, Count = c }).ForEach(pair =>
					{
						ApiRouter.Participants.Delete(message.Authenticator, pair.Participant.Id);

						var countMessage = new EngineMessage
						{
							Counter = counter,
							Count = pair.Count,
							Total = gCount,
							Item = pair.Participant
						};
						SingleProcessors.Core.ProcessCount(countMessage);
					});
				}
			});
			return message;
		};

		public Func<EngineMessage, EngineMessage> Delete = message =>
		{
			ApiRouter.Participants.Delete(message.Authenticator, message.Item.Id);
			return message;
		};	

		public Func<EngineMessage, EngineMessage> EditById = message =>
		{
			var uniqueId = message.GetValueFromRow("UniqueId");
			
			var participant = ParticipantsRepository.GetManyDetailed(message.Authenticator).FirstOrDefault(p =>
				{
					var visualBlock = p.VisualBlockValueLines;
					if (visualBlock == null) return false;
					return visualBlock
						.SelectMany(line => line.Values.Select(value => value.Value))
						.Contains(uniqueId);
				}
			);
			if (participant == null) return null;

			var editedParticipant = EditParticipant(message.Table, participant, message.Authenticator, message.Row,
				message.Settings.SearchKey);
			if (editedParticipant == null) return null;

			var result = ApiRouter.Participants.Create(message.Authenticator, editedParticipant);

			return new EngineMessage { Item = result };
		};

		public Func<EngineMessage, EngineMessage> Edit = message =>
		{
			var participantName = message.GetValueFromRow("Participant");
			var detailedParticipant = ParticipantsRepository.GetDetailed(message.Authenticator, participantName);
			if (detailedParticipant == null) return null;

			var editedParticipant = EditParticipant(message.Table, detailedParticipant, message.Authenticator, message.Row,
				message.Settings.SearchKey);
			if (editedParticipant == null) return null;

			var result = ApiRouter.Participants.Create(message.Authenticator, editedParticipant);

			return new EngineMessage { Item = result };
		};
	}
}
