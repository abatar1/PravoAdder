using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fclp.Internals.Extensions;
using PravoAdder.Api.Domain;
using PravoAdder.Domain;
using PravoAdder.Helpers;
using PravoAdder.Readers;
using PravoAdder.Wrappers;

namespace PravoAdder.Processors
{
	public class SingleProcessors
	{
		public static Func<EngineMessage, EngineMessage> UpdateProject = message =>
		{			
			var response = TryCreateProject(message);
			return null;
		};

		public static Func<EngineMessage, EngineMessage> TryCreateProject = message =>
		{
			var headerBlock = message.CaseCreator.ReadHeaderBlock(message.Row);
			if (headerBlock == null) return null;

			var projectGroup = message.ApiEnviroment.AddProjectGroup(message.ApplicationArguments.IsOverwrite, headerBlock);
			var project = message.ApiEnviroment.AddProject(message.ApplicationArguments.IsOverwrite, headerBlock, projectGroup?.Id, message.Count, message.IsUpdate);

			return new EngineMessage { HeaderBlock = headerBlock, Item = project };
		};

		public static Func<EngineMessage, EngineMessage> LoadSettings = message =>
		{
			var settingsController = new SettingsWrapper();
			return new EngineMessage
			{
				Settings = settingsController.LoadSettingsFromConsole(message.ApplicationArguments),
				ParallelOptions = new ParallelOptions { MaxDegreeOfParallelism = message.ApplicationArguments.ParallelOptions }
			};
		};

		public static Func<EngineMessage, EngineMessage> LoadTable = message =>
		{
			TableEnviroment.Initialize(message.ApplicationArguments, message.Settings);
			return new EngineMessage { Table = TableEnviroment.Table, Settings = message.Settings };
		};

		public static Func<EngineMessage, EngineMessage> InitializeApp = message =>
		{
			var authenticatorController = new AuthentificatorWrapper(message.Settings);
			var authenticator = authenticatorController.Authenticate();
			var processType = message.ApplicationArguments.ProcessType;

			return new EngineMessage
			{
				Authenticator = authenticator,
				CaseCreator = new CaseCreator(message.Table, message.Settings, authenticator),
				ApiEnviroment = new ApiEnviroment(authenticator),
				Counter = new Counter(),
				TaskCreator = processType == ProcessType.CreateTask ? new TaskCreator(authenticator) : null,
				ParticipantCreator = processType == ProcessType.CreateParticipant
					? new ParticipantCreator(authenticator, message.ApplicationArguments.ParticipantType)
					: null
			};
		};

		public static Func<EngineMessage, EngineMessage> DeleteProject = message =>
		{
			message.ApiEnviroment.DeleteProjectItem(message.Item.Id);
			return new EngineMessage();
		};

		public static Func<EngineMessage, EngineMessage> DeleteFolder = message =>
		{
			message.ApiEnviroment.DeleteProjectItem(message.Item.Id);
			return new EngineMessage();
		};

		public static Func<EngineMessage, EngineMessage> DeleteParticipant = message =>
		{
			message.ApiEnviroment.DeleteParticipant(message.Item.Id);
			return new EngineMessage();
		};

		public static Func<EngineMessage, EngineMessage> DeleteProjectGroup = message =>
		{
			if (!message.Item.Equals(ProjectGroup.Empty))
			{
				message.ApiEnviroment.DeleteProjectGroupItem(message.Item.Id);
			}
			return new EngineMessage();
		};

		public static Func<EngineMessage, EngineMessage> ProcessCount = message =>
		{
			message.Counter.ProcessCount(message.Count, message.Total, message.Item, 70);
			return new EngineMessage();
		};

		public static Func<EngineMessage, EngineMessage> AddInformation = message =>
		{
			var blocksInfo = message.CaseCreator.Create();
			foreach (var repeatBlock in blocksInfo)
			{
				foreach (var blockInfo in repeatBlock.Blocks)
				{
					message.ApiEnviroment.AddInformation(blockInfo, message.Row, message.Item.Id, repeatBlock.Order);
				}
			}
			return new EngineMessage();
		};

		public static Func<EngineMessage, EngineMessage> SynchronizeProject = message =>
		{
			var blocksInfo = message.CaseCreator.Create();
			foreach (var repeatBlock in blocksInfo)
			{
				foreach (var blockInfo in repeatBlock.Blocks)
				{
					message.ApiEnviroment.AddInformation(blockInfo, message.Row, message.Item.Id, repeatBlock.Order);
				}
			}
			return new EngineMessage();
		};

		public static Func<EngineMessage, EngineMessage> CreateTask = message =>
		{
			var task = message.TaskCreator.Create(message.Table.Header, message.Row);
			message.ApiEnviroment.CreateTask(task);
			if (task.IsArchive) message.ApiEnviroment.ArchiveProject(task.Project.Id);
			return new EngineMessage();
		};

		private static List<Participant> _participants;

		public static Func<EngineMessage, EngineMessage> CreateParticipant = message =>
		{
			if (_participants == null) _participants = message.ApiEnviroment.GetParticipants();

			var newParticipant = message.ParticipantCreator.Create(message.Table.Header, message.Row);
			if (_participants.FirstOrDefault(p => p.Name == newParticipant.FullName) != null) return null;
			message.ApiEnviroment.PutExtendentParticipant(newParticipant);
			return new EngineMessage();
		};

		public static Func<EngineMessage, EngineMessage> DistinctParticipants = message =>
		{
			var response = message.ApiEnviroment.GetParticipants();
			var counter = message.Counter.CloneJson();
			response.GroupBy(p => p.Name).ForEach(g =>
			{
				if (g.Count() > 1)
				{
					var gCount = g.Count() - 1;
					g.Skip(1).Select((p, c) => new { Participant = p, Count = c }).ForEach(pair =>
					{
						message.ApiEnviroment.DeleteParticipant(pair.Participant.Id);

						var countMessage = new EngineMessage
						{
							Counter = counter,
							Count = pair.Count,
							Total = gCount,
							Item = pair.Participant
						};
						ProcessCount(countMessage);
					});
				}
			});
			return new EngineMessage();
		};
	}
}