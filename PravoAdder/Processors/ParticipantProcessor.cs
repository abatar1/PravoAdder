using System;
using System.Collections.Generic;
using System.Linq;
using Fclp.Internals.Extensions;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Domain;
using PravoAdder.Helpers;
using PravoAdder.Readers;

namespace PravoAdder.Processors
{
	public class ParticipantProcessor
	{
		private static List<Lazy<DetailedParticipant>> _detailedParticipants;
		private static List<VisualBlockLine> _blockLines;
		private static List<Participant> _participants;

		private static void SetDetailedParticipant(HttpAuthenticator authenticator)
		{
			if (_participants == null) _participants = ApiRouter.Participants.GetParticipants(authenticator);
			_detailedParticipants = _participants
				.Where(p => p.TypeName == ParticipantCreator.Person)
				.Select(p => new Lazy<DetailedParticipant>(() => ApiRouter.Participants.GetParticipant(authenticator, p.Id)))
				.ToList();		
		}

		private static DetailedParticipant EditParticipant(DetailedParticipant participant, HttpAuthenticator authenticator, Row header, Row row, string searchKey)
		{
			if (participant.VisualBlockValueLines == null) participant.VisualBlockValueLines = new List<VisualBlockParticipantLine>();

			if (_blockLines == null)
			{
				var fParticipantId = _detailedParticipants.First().Value.Id;
				_blockLines = ApiRouter.Participants.GetVisualBlock(authenticator, fParticipantId).Lines.ToList();
			}

			var fieldValue = Table.GetValue(header, row, searchKey)?.Trim();
			if (string.IsNullOrEmpty(fieldValue)) return null;

			var editingLine = _blockLines.FirstOrDefault(line => line.Fields.Any(field =>
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

		public static DetailedParticipant GetParticipantByName(HttpAuthenticator authenticator, Row header, Row row, string name)
		{
			if (_participants == null) _participants = ApiRouter.Participants.GetParticipants(authenticator);
			var participant = _participants.FirstOrDefault(p => p.Name == name);
			if (participant == null) return null;
			return ApiRouter.Participants.GetParticipant(authenticator, participant.Id);
		}

		public Func<EngineMessage, EngineMessage> Create = message =>
		{
			if (_participants == null) _participants = ApiRouter.Participants.GetParticipants(message.Authenticator);

			var newParticipant = (DetailedParticipant) message.GetCreatable();
			var existedParticipant = _participants.FirstOrDefault(p => p.Name == newParticipant.FullName);
			if (existedParticipant != null) return new EngineMessage { Item = existedParticipant };
			var participantResponse = ApiRouter.Participants.PutParticipant(message.Authenticator, newParticipant);
			return new EngineMessage { Item = participantResponse };
		};

		public Func<EngineMessage, EngineMessage> Distinct = message =>
		{
			var response = ApiRouter.Participants.GetParticipants(message.Authenticator);
			var counter = message.Counter.CloneJson();
			response.GroupBy(p => p.Name).ForEach(g =>
			{
				if (g.Count() > 1)
				{
					var gCount = g.Count() - 1;
					g.Skip(1).Select((p, c) => new { Participant = p, Count = c }).ForEach(pair =>
					{
						ApiRouter.Participants.DeleteParticipant(message.Authenticator, pair.Participant.Id);

						var countMessage = new EngineMessage
						{
							Counter = counter,
							Count = pair.Count,
							Total = gCount,
							Item = pair.Participant
						};
						SingleProcessors.ProcessCount(countMessage);
					});
				}
			});
			return message;
		};

		public Func<EngineMessage, EngineMessage> Delete = message =>
		{
			ApiRouter.Participants.DeleteParticipant(message.Authenticator, message.Item.Id);
			return message;
		};	

		public Func<EngineMessage, EngineMessage> EditById = message =>
		{
			SetDetailedParticipant(message.Authenticator);

			var uniqueId = Table.GetValue(message.Table.Header, message.Row, "UniqueId");
			var participant = _detailedParticipants.FirstOrDefault(p =>
				{
					var visualBlock = p.Value.VisualBlockValueLines;
					if (visualBlock == null) return false;
					return visualBlock
						.SelectMany(line => line.Values.Select(value => value.Value))
						.Contains(uniqueId);
				}
			)?.Value;
			if (participant == null) return null;

			var editedParticipant = EditParticipant(participant, message.Authenticator, message.Table.Header, message.Row, message.Args.SearchKey);
			if (editedParticipant == null) return null;

			var result = ApiRouter.Participants.PutParticipant(message.Authenticator, editedParticipant);

			return new EngineMessage { Item = result };
		};

		public Func<EngineMessage, EngineMessage> Edit = message =>
		{
			SetDetailedParticipant(message.Authenticator);

			var participantName = Table.GetValue(message.Table.Header, message.Row, "Participant");
			var detailedParticipant = GetParticipantByName(message.Authenticator, message.Table.Header, message.Row, participantName);
			if (detailedParticipant == null) return null;

			var editedParticipant = EditParticipant(detailedParticipant, message.Authenticator, message.Table.Header, message.Row, message.Args.SearchKey);
			if (editedParticipant == null) return null;

			var result = ApiRouter.Participants.PutParticipant(message.Authenticator, editedParticipant);

			return new EngineMessage { Item = result };
		};
	}
}
