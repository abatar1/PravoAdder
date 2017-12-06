using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Repositories;
using PravoAdder.Domain;

namespace PravoAdder.Processors
{
	public class ForEachProcessors
	{
		private static EngineMessage ProcessForEach<T>(IReadOnlyCollection<T> items, EngineMessage message,
			Func<EngineMessage, object, EngineMessage> messageProcessor, Func<T, bool> continuationProcessor = null)
		{
			Parallel.ForEach(items, message.ParallelOptions, (item, state, index) =>
			{
				if (continuationProcessor != null && continuationProcessor(item)) return;

				for (var i = 0; i < message.Child.Count; i++)
				{
					var childConveyer = message.Child[i];
					if (childConveyer.Message.Table == null)
					{
						childConveyer.Message.Table = message.Table;
					}
					
					var itemizedMessage = messageProcessor(childConveyer.Message, item);
					itemizedMessage.Count = (int) index;
					itemizedMessage.Total = items.Count;

					var newMessage = childConveyer.Processor.Invoke(itemizedMessage);
					if (i < message.Child.Count - 1)
					{
						message.Child[i + 1].Message.Item = newMessage?.Item;
						message.Child[i + 1].Message.Table = newMessage?.Table;
						message.Child[i + 1].Message.HeaderBlock = newMessage?.HeaderBlock;
					}
				}
			});
			return new EngineMessage();
		}

		public static Func<EngineMessage, EngineMessage> File = message =>
		{			
			var allfiles = Directory.GetFiles(message.Args.SourceName, "*.*", SearchOption.AllDirectories);
			return ProcessForEach(allfiles, message, (msg, filename) =>
			{
				msg.Args.SourceName =  (string) filename;
				return msg;
			});
		};

		public static Func<EngineMessage, EngineMessage> Row = message =>
		{			
			var rows = message.Table.TableContent.ToList();
			return ProcessForEach(rows, message, (msg, item) =>
			{
				var row = (Row) item;
				msg.Row = row;
				msg.HeaderBlock = msg.CaseBuilder.ReadHeaderBlock(row);
				return msg;
			});
		};

		public static Func<EngineMessage, EngineMessage> Project = message =>
		{
			var projects = ApiRouter.Projects.GetMany(message.Authenticator, message.Item.Id);
			return ProcessForEach(projects, message, (msg, item) =>
			{
				msg.Item = (Project) item;
				return msg;
			});
		};

		public static Func<EngineMessage, EngineMessage> Event = message =>
		{
			var projects = ApiRouter.Events.GetMany(message.Authenticator);
			return ProcessForEach(projects, message, (msg, item) =>
			{
				msg.Item = (GroupItem) item;
				return msg;
			});
		};

		public static Func<EngineMessage, EngineMessage> ProjectByDate = message =>
		{
			var projects = ApiRouter.Projects.GetMany(message.Authenticator, message.Item.Id)
				.Where(p => p.CreationDate == DateTime.Parse(message.Args.Date))
				.ToList();
			return ProcessForEach(projects, message, (msg, item) =>
			{
				msg.Item = (Project) item;
				return msg;
			});
		};

		public static Func<EngineMessage, EngineMessage> ProjectByType = message =>
		{
			var type = ApiRouter.ProjectTypes.GetMany(message.Authenticator)
				.FirstOrDefault(t => t.Name.Equals(message.Args.ProjectType.Replace('_', ' ')));
			if (type == null)
			{
				message.IsFinal = true;
				return message;
			}

			var projects = ApiRouter.Projects.GetMany(message.Authenticator, message.Item.Id)
				.Where(p => p.ProjectType.Equals(type))
				.ToList();
			return ProcessForEach(projects, message, (msg, item) =>
			{
				msg.Item = (Project)item;
				return msg;
			});
		};

		public static Func<EngineMessage, EngineMessage> Folder = message =>
		{
			var folders = ApiRouter.ProjectFolders.GetMany(message.Authenticator);
			return ProcessForEach(folders, message,
				(msg, item) =>
				{
					msg.Item = (ProjectFolder) item;
					return msg;
				},
				item => ApiRouter.Projects.GetGroupedMany(message.Authenticator, item.Name) == null);
		};

		public static Func<EngineMessage, EngineMessage> ProjectGroup = message =>
		{
			var projectGroups = ProjectGroupRepository.GetMany<ProjectGroupsApi>(message.Authenticator);
			return ProcessForEach(projectGroups, message, (msg, item) =>
			{
				msg.Item = (ProjectGroup) item;
				return msg;
			});
		};

		public static Func<EngineMessage, EngineMessage> Participant = message =>
		{
			var participants = ApiRouter.Participants.GetMany(message.Authenticator);
			return ProcessForEach(participants, message, (msg, item) =>
			{
				msg.Item = (Participant) item;
				return msg;
			});
		};

		public static Func<EngineMessage, EngineMessage> ParticipantByDate = message =>
		{
			var neededDatetime = DateTime.Parse(message.Args.Date).ToString("d");
			var participants = ApiRouter.Participants.GetMany(message.Authenticator)
				.Select(p => ApiRouter.Participants.Get(message.Authenticator, p.Id))
				.Where(p => DateTime.Parse(p.CreationDate).ToString("d") == neededDatetime)
				.ToList();

			return ProcessForEach(participants, message, (msg, item) =>
			{
				msg.Item = (Participant) item;
				return msg;
			});
		};
	}
}