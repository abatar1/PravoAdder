using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Domain;

namespace PravoAdder.Processors
{
	public class ForEachProcessors
	{
		//TODO Точно работает только с depth <= 2. Нужно понять, будет ли это работать с любой глубиной.
		private static EngineMessage ProcessForEach<T>(IReadOnlyCollection<T> items, EngineMessage message,
			Func<EngineMessage, object, EngineMessage> messageProcessor, Func<T, bool> continuationProcessor = null)
		{
			Parallel.ForEach(items, message.ParallelOptions, (item, state, index) =>
			{
				if (continuationProcessor != null && continuationProcessor(item)) return;

				foreach (var childConveyer in message.Child)
				{
					var itemizedMessage = messageProcessor(childConveyer.Message, item);
					itemizedMessage.Count = (int) index;
					itemizedMessage.Total = items.Count;
					childConveyer.Processor.Invoke(itemizedMessage);
				}
			});
			return new EngineMessage();
		}

		public static Func<EngineMessage, EngineMessage> Row = message =>
		{
			var rows = message.Table.TableContent;
			return ProcessForEach(rows, message, (msg, item) =>
			{
				msg.Row = (Row) item;
				return msg;
			});
		};

		public static Func<EngineMessage, EngineMessage> Project = message =>
		{
			var projects = message.ApiEnviroment.GetGroupedProjects(message.Item.Id)
				.SelectMany(s => s.Projects)
				.ToList();
			return ProcessForEach(projects, message, (msg, item) =>
			{
				msg.Item = (Project) item;
				return msg;
			});
		};

		public static Func<EngineMessage, EngineMessage> ProjectByDate = message =>
		{
			var projects = message.ApiEnviroment.GetGroupedProjects(message.Item.Id)
				.SelectMany(s => s.Projects)
				.Where(p => p.CreationDate == DateTime.Parse(message.ApplicationArguments.Date))
				.ToList();
			return ProcessForEach(projects, message, (msg, item) =>
			{
				msg.Item = (Project) item;
				return msg;
			});
		};

		public static Func<EngineMessage, EngineMessage> Folder = message =>
		{
			var folders = message.ApiEnviroment.GetProjectFolderItems();
			return ProcessForEach(folders, message,
				(msg, item) => {
					msg.Item = (ProjectFolder) item;
					return msg;
				},
				item => message.ApiEnviroment.GetGroupedProjects(null, item.Name) == null);
		};

		public static Func<EngineMessage, EngineMessage> ProjectGroup = message =>
		{
			var projectGroups = message.ApiEnviroment.GetProjectGroupItems();
			return ProcessForEach(projectGroups, message, (msg, item) =>
			{
				msg.Item = (ProjectGroup) item;
				return msg;
			});
		};

		public static Func<EngineMessage, EngineMessage> Participant = message =>
		{
			var participants = message.ApiEnviroment.GetParticipants();
			return ProcessForEach(participants, message, (msg, item) =>
			{
				msg.Item = (Participant) item;
				return msg;
			});
		};

		public static Func<EngineMessage, EngineMessage> ParticipantByDate = message =>
		{
			var neededDatetime = DateTime.Parse(message.ApplicationArguments.Date).ToString("d");
			var participants = message.ApiEnviroment.GetParticipants()
				.Select(p => ApiRouter.Participants.GetParticipant(message.Authenticator, p.Id))
				.Where(p => DateTime.Parse(p.CreationDate).ToString("d") == neededDatetime)
				.ToList();

			return ProcessForEach(participants, message, (msg, item) =>
			{
				msg.Item = (Participant)item;
				return msg;
			});
		};
	}
}