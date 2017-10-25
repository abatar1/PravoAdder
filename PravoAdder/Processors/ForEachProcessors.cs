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

				for (var i = 0; i < message.Child.Count; i++)
				{
					var childConveyer = message.Child[i];
					var itemizedMessage = messageProcessor(childConveyer.Message, item);
					itemizedMessage.Count = (int)index;
					itemizedMessage.Total = items.Count;
					var newMessage = childConveyer.Processor.Invoke(itemizedMessage);
					if (i < message.Child.Count - 1)
					{
						message.Child[i + 1].Message.Item = newMessage?.Item;
					}
				}
			});
			return new EngineMessage();
		}

		public static Func<EngineMessage, EngineMessage> Row = message =>
		{
			var rows = message.Table.TableContent.ToList();
			return ProcessForEach(rows, message, (msg, item) =>
			{
				msg.Row = (Row) item;
				return msg;
			});
		};

		public static Func<EngineMessage, EngineMessage> Project = message =>
		{
			var projects = ApiRouter.Projects.GetProjects(message.Authenticator, message.Item.Id);
			return ProcessForEach(projects, message, (msg, item) =>
			{
				msg.Item = (Project) item;
				return msg;
			});
		};

		public static Func<EngineMessage, EngineMessage> ProjectByDate = message =>
		{
			var projects = ApiRouter.Projects.GetProjects(message.Authenticator, message.Item.Id)
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
			var folders = ApiRouter.ProjectFolders.GetProjectFolders(message.Authenticator);
			return ProcessForEach(folders, message,
				(msg, item) =>
				{
					msg.Item = (ProjectFolder) item;
					return msg;
				},
				item => ApiRouter.Projects.GetGroupedProjects(message.Authenticator, item.Name) == null);
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
			var participants = ApiRouter.Participants.GetParticipants(message.Authenticator);
			return ProcessForEach(participants, message, (msg, item) =>
			{
				msg.Item = (Participant) item;
				return msg;
			});
		};

		public static Func<EngineMessage, EngineMessage> ParticipantByDate = message =>
		{
			var neededDatetime = DateTime.Parse(message.ApplicationArguments.Date).ToString("d");
			var participants = ApiRouter.Participants.GetParticipants(message.Authenticator)
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