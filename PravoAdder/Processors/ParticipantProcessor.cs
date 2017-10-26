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
		private static List<Lazy<ExtendentParticipant>> _lazyParticipants;
		private static List<VisualBlockLine> _blockLines;
		private static List<Participant> _participants;

		private static ExtendentParticipant GetByKeyname(HttpAuthenticator authenticator, Row header, Row row, string uniqueIdFieldName)
		{
			if (_lazyParticipants == null)
			{
				_lazyParticipants = ApiRouter.Participants.GetParticipants(authenticator)
					.Where(p => p.TypeName == ParticipantCreator.Person)
					.Select(p => new Lazy<ExtendentParticipant>(() => ApiRouter.Participants.GetParticipant(authenticator, p.Id)))
					.ToList();
			}

			var uniqueId = Table.GetValue(header, row, uniqueIdFieldName);
			var participant = _lazyParticipants.FirstOrDefault(p =>
				{
					var visualBlock = p.Value.VisualBlockValueLines;
					if (visualBlock == null) return false;
					return visualBlock
						.SelectMany(line => line.Values.Select(value => value.Value))
						.Contains(uniqueId);
				}
			)?.Value;

			return participant;
		}

		public Func<EngineMessage, EngineMessage> Create = message =>
		{
			if (_participants == null) _participants = ApiRouter.Participants.GetParticipants(message.Authenticator);

			var newParticipant = message.ParticipantCreator.Create(message.Table.Header, message.Row);
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

		public Func<EngineMessage, EngineMessage> Edit = message =>
		{
			var participant = GetByKeyname(message.Authenticator, message.Table.Header, message.Row, "UniqueId");
			if (participant == null) return null;

			if (_blockLines == null)
			{
				var fParticipantId = _lazyParticipants.First().Value.Id;
				_blockLines = ApiRouter.Participants.GetVisualBlock(message.Authenticator, fParticipantId).Lines.ToList();
			}

			const string noteFieldName = "Notes";
			var noteText = Table.GetValue(message.Table.Header, message.Row, noteFieldName)?.Trim();
			if (string.IsNullOrEmpty(noteText)) return null;

			var editingLine = _blockLines.FirstOrDefault(line => line.Fields.Any(field =>
			{
				var isRequiredFieldName = field.ProjectField.Name.Contains(noteFieldName);
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
						if (f.ProjectField.Name.Contains(noteFieldName)) newField.Value = noteText;
						return newField;
					})
					.ToList()
			};
			participant.VisualBlockValueLines.Add(addingLine);

			var result = ApiRouter.Participants.PutParticipant(message.Authenticator, participant);

			return new EngineMessage { Item = result };
		};
	}
}
