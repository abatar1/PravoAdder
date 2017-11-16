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
		private static Participant EditParticipant(Participant participant, HttpAuthenticator authenticator, Row header, Row row, string searchKey)
		{
			if (participant.VisualBlockValueLines == null) participant.VisualBlockValueLines = new List<VisualBlockParticipantLine>();
		
			var blockLines = ParticipantsRepository.Get<ParticipantsApi>(authenticator, participant.Id).VisualBlock.Lines;			

			var fieldValue = Table.GetValue(header, row, searchKey)?.Trim();
			if (string.IsNullOrEmpty(fieldValue)) return null;

			var editingLine = blockLines.FirstOrDefault(line => line.Fields.Any(field =>
			{
				var isRequiredFieldName = field.ProjectField.Name.Contains(searchKey);
				var doesAlreadyContains = participant.VisualBlockValueLines.FirstOrDefault(pl => pl.BlockLineId == line.Id) != null;
				return isRequiredFieldName && !doesAlreadyContains;
			}));
			if (editingLine == null) return null;

			var addingLine = new VisualBlockParticipantLine
			{
				BlockLineId = editingLine.Id,
				Values = editingLine.Fields
					.Select(f =>
					{
						var newField = new VisualBlockParticipantField { VisualBlockProjectFieldId = f.Id };
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
			var newParticipant = (Participant) message.GetCreatable();
			var existedParticipant =
				ParticipantsRepository.GetOrCreate<ParticipantsApi>(message.Authenticator, newParticipant.FullName, newParticipant);
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
			var uniqueId = Table.GetValue(message.Table.Header, message.Row, "UniqueId");
			
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

			var editedParticipant = EditParticipant(participant, message.Authenticator, message.Table.Header, message.Row, message.Args.SearchKey);
			if (editedParticipant == null) return null;

			var result = ApiRouter.Participants.Create(message.Authenticator, editedParticipant);

			return new EngineMessage { Item = result };
		};

		public Func<EngineMessage, EngineMessage> Edit = message =>
		{
			var participantName = Table.GetValue(message.Table.Header, message.Row, "Participant");
			var detailedParticipant = ParticipantsRepository.GetDetailed<ParticipantsApi>(message.Authenticator, participantName);
			if (detailedParticipant == null) return null;

			var editedParticipant = EditParticipant(detailedParticipant, message.Authenticator, message.Table.Header, message.Row, message.Args.SearchKey);
			if (editedParticipant == null) return null;

			var result = ApiRouter.Participants.Create(message.Authenticator, editedParticipant);

			return new EngineMessage { Item = result };
		};
	}
}
