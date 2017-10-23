using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fclp.Internals.Extensions;
using OfficeOpenXml;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Domain;
using PravoAdder.Domain.Attributes;
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
			return new EngineMessage {Item = response.Item};
		};

		public static Func<EngineMessage, EngineMessage> TryCreateProject = message =>
		{
			var headerBlock = message.CaseCreator.ReadHeaderBlock(message.Row);
			if (headerBlock == null) return null;

			var projectGroup = message.ApiEnviroment.AddProjectGroup(message.ApplicationArguments.IsOverwrite, headerBlock);
			var project = message.ApiEnviroment.AddProject(message.ApplicationArguments.IsOverwrite, headerBlock,
				projectGroup?.Id, message.Count, message.IsUpdate);

			return new EngineMessage {HeaderBlock = headerBlock, Item = project};
		};

		public static Func<EngineMessage, EngineMessage> LoadSettings = message =>
		{
			var settingsController = new SettingsWrapper();
			return new EngineMessage
			{
				Settings = settingsController.LoadSettingsFromConsole(message.ApplicationArguments),
				ParallelOptions = new ParallelOptions {MaxDegreeOfParallelism = message.ApplicationArguments.ParallelOptions}
			};
		};

		public static Func<EngineMessage, EngineMessage> LoadTable = message =>
		{
			TableEnviroment.Initialize(message.ApplicationArguments, message.Settings);
			return new EngineMessage {Table = TableEnviroment.Table, Settings = message.Settings};
		};

		public static Func<EngineMessage, EngineMessage> InitializeApp = message =>
		{
			var authenticatorController = new AuthentificatorWrapper(message.Settings, message.ApplicationArguments);
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
			return message;
		};

		public static Func<EngineMessage, EngineMessage> DeleteFolder = message =>
		{
			message.ApiEnviroment.DeleteProjectItem(message.Item.Id);
			return message;
		};

		public static Func<EngineMessage, EngineMessage> DeleteParticipant = message =>
		{
			ApiRouter.Participants.DeleteParticipant(message.Authenticator, message.Item.Id);
			return message;
		};

		public static Func<EngineMessage, EngineMessage> DeleteProjectGroup = message =>
		{
			if (!message.Item.Equals(ProjectGroup.Empty))
			{
				message.ApiEnviroment.DeleteProjectGroupItem(message.Item.Id);
			}
			return message;
		};

		public static Func<EngineMessage, EngineMessage> ProcessCount = message =>
		{
			if (message.Item == null) return message;
			message.Counter.ProcessCount(message.Count, message.Total, message.Item, 70);
			return message;
		};

		public static Func<EngineMessage, EngineMessage> AddInformation = message =>
		{
			if (message.Item == null) return message;

			var blocksInfo = message.CaseCreator.Create();
			foreach (var repeatBlock in blocksInfo)
			{
				foreach (var blockInfo in repeatBlock.Blocks)
				{
					message.ApiEnviroment.AddInformation(blockInfo, message.Row, message.Item.Id, repeatBlock.Order);
				}
			}
			return message;
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
			return message;
		};

		public static Func<EngineMessage, EngineMessage> CreateTask = message =>
		{
			var task = message.TaskCreator.Create(message.Table.Header, message.Row);
			ApiRouter.Task.Create(message.Authenticator, task);
			if (task.IsArchive) ApiRouter.Projects.ArchiveProject(message.Authenticator, task.Project.Id);
			return message;
		};

		private static List<Participant> _participants;

		public static Func<EngineMessage, EngineMessage> CreateParticipant = message =>
		{
			if (_participants == null) _participants = ApiRouter.Participants.GetParticipants(message.Authenticator);

			var newParticipant = message.ParticipantCreator.Create(message.Table.Header, message.Row);
			var existedParticipant = _participants.FirstOrDefault(p => p.Name == newParticipant.FullName);
			if (existedParticipant != null) return new EngineMessage {Item = existedParticipant};
			var participantResponse = ApiRouter.Participants.PutParticipant(message.Authenticator, newParticipant);
			return new EngineMessage {Item = participantResponse};
		};

		public static Func<EngineMessage, EngineMessage> DistinctParticipants = message =>
		{
			var response = ApiRouter.Participants.GetParticipants(message.Authenticator);
			var counter = message.Counter.CloneJson();
			response.GroupBy(p => p.Name).ForEach(g =>
			{
				if (g.Count() > 1)
				{
					var gCount = g.Count() - 1;
					g.Skip(1).Select((p, c) => new {Participant = p, Count = c}).ForEach(pair =>
					{
						ApiRouter.Participants.DeleteParticipant(message.Authenticator, pair.Participant.Id);

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
			return message;
		};

		public static Func<EngineMessage, EngineMessage> AnalyzeHeader = message =>
		{
			var types = ApiRouter.ProjectTypes.GetProjectTypes(message.Authenticator);
			var blockTypes = new Dictionary<string, List<(string, List<string>)>>();
			var errorKeys = new List<int>();
			foreach (var cell in message.Table.Header)
			{
				if (cell.Value.IsSystem)
				{
					var systemHeaderName = typeof(HeaderBlockInfo).GetProperties()
						.Select(p => p.LoadAttribute<FieldNameAttribute>().FieldNames)
						.FirstOrDefault(p => p.Contains(cell.Value.FieldName));
					if (systemHeaderName != null) continue;
				}

				var wasAchieved = false;
				foreach (var type in types)
				{
					if (!blockTypes.ContainsKey(type.Name))
					{
						var blocks = (from block in ApiRouter.ProjectTypes.GetVisualBlocks(message.Authenticator, type.Id)
							let fields = block.Lines.SelectMany(line => line.Fields.Select(field => field.ProjectField.Name)).ToList()
							select (block.Name, fields)).ToList();
						blockTypes.Add(type.Name, blocks);
					}
					var blockName = blockTypes[type.Name]
						.FirstOrDefault(block => block.Item1 == cell.Value.BlockName && block.Item2.Contains(cell.Value.FieldName)).Item1;
					if (blockName != null)
					{
						wasAchieved = true;
						break;
					}			
				}
				if (!wasAchieved)
				{
					errorKeys.Add(cell.Key);			
				}
			}
			using (var xlPackage = new ExcelPackage(new FileInfo(message.ApplicationArguments.SourceFileName + ".xlsx")))
			{
				var worksheet = xlPackage.Workbook.Worksheets.First();
				foreach (var key in errorKeys)
				{
					worksheet.Cells[message.Settings.InformationRowPosition, key].Style.Fill.BackgroundColor.SetColor(Color.Red);
				}			
				xlPackage.Save();
			}
			return message;
		};
	}
}