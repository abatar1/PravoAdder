using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Repositories;
using PravoAdder.Domain;

namespace PravoAdder.Processors
{
	public class ProjectProcessor
	{
		public Func<EngineMessage, EngineMessage> Update = message =>
		{
			var headerBlock = message.CaseBuilder.ReadHeaderBlock(message.Row);
			var project = ProjectRepository.Get<ProjectsApi>(message.Authenticator, headerBlock.Name);

			return new EngineMessage { HeaderBlock = headerBlock, Item = project };
		};

		public Func<EngineMessage, EngineMessage> TryCreate = message =>
		{
			var headerBlock = message.CaseBuilder.ReadHeaderBlock(message.Row);
			if (string.IsNullOrEmpty(headerBlock.Name) || string.IsNullOrEmpty(headerBlock.ProjectType)) return null;

			var projectGroup = message.ApiEnviroment.AddProjectGroup(message.Args.IsOverwrite, headerBlock);
			var project = message.ApiEnviroment.AddProject(message.Args.IsOverwrite, headerBlock,
				projectGroup?.Id, message.Count, message.IsUpdate);

			return new EngineMessage { HeaderBlock = headerBlock, Item = project };
		};

		public Func<EngineMessage, EngineMessage> Delete = message =>
		{
			message.ApiEnviroment.DeleteProjectItem(message.Item.Id);
			return message;
		};

		public Func<EngineMessage, EngineMessage> Rename = message =>
		{
			var projectName = Table.GetValue(message.Table.Header, message.Row, "Case name");
			var project = ProjectRepository.Get<ProjectsApi>(message.Authenticator, projectName);
			if (project == null) return null;

			project.Name = Table.GetValue(message.Table.Header, message.Row, "New case name");
			message.Item = ApiRouter.Projects.Put(message.Authenticator, project);
			return message;
		};

		public Func<EngineMessage, EngineMessage> AddNote = message =>
		{
			var projectName = message.GetValueFromRow("Case Name");

			var project = ProjectRepository.Get<ProjectsApi>(message.Authenticator, projectName);
			if (project == null) return null;

			var note = new Note
			{
				Project = project,
				Text = message.GetValueFromRow("Note"),
				IsPrivate = bool.Parse(message.GetValueFromRow("Is Private"))
			};
			ApiRouter.Notes.Create(message.Authenticator, note);
			return new EngineMessage { Item = project };
		};

		public Func<EngineMessage, EngineMessage> Synchronize = message =>
		{
			if (!string.IsNullOrEmpty(message.HeaderBlock.CasebookNumber))
			{			
				var project = ProjectRepository.Get<ProjectsApi>(message.Authenticator, message.HeaderBlock.Name);
				if (project == null) return null;

				var asyncResult = ApiRouter.Casebook.CheckAsync(message.Authenticator, project.Id,
					message.HeaderBlock.CasebookNumber).Result;
				message.Item = project;
				return message;
			}
			return null;
		};

		public Func<EngineMessage, EngineMessage> AddInformation = message =>
		{
			if (message.Item == null) return message;

			var blocksInfo = message.CaseBuilder.Build(((Project) message.Item).ProjectType);
			if (blocksInfo == null) return null;

			var projectId = message.Item.Id;			

			foreach (var repeatBlock in blocksInfo)
			{
				foreach (var blockInfo in repeatBlock.VisualBlocks)
				{
					var projectVisualBlock = ApiRouter.ProjectCustomValues.GetAllVisualBlocks(message.Authenticator, projectId)
						.Blocks.First(x => x.Name.Equals(blockInfo.NameInConstructor));
					message.ApiEnviroment.AddInformation(blockInfo, message.Row, projectId, repeatBlock.Order);
				}
			}
			return message;
		};

		public Func<EngineMessage, EngineMessage> UpdateInformation = message =>
		{
			if (message.Item == null) return message;

			var projectType = ((Project) message.Item).ProjectType;
			var blocksInfo = message.CaseBuilder.Build(projectType);
			if (blocksInfo == null) return null;

			var projectId = message.Item.Id;

			var successFlag = false;
			foreach (var repeatBlock in blocksInfo)
			{
				foreach (var blockInfo in repeatBlock.VisualBlocks)
				{
					var blockName = blockInfo.Name;
					if (blockInfo.Name.Contains("-"))
					{
						blockName = blockInfo.Name.Remove(blockInfo.Name.IndexOf("-", StringComparison.Ordinal)).Trim();
					}

					var projectVisualBlocks = ApiRouter.ProjectCustomValues.GetAllVisualBlocks(message.Authenticator, projectId)
						.Blocks.Where(x => x.Name.Contains(blockName)).ToList();

					if (projectVisualBlocks.Count > 0)
					{
						foreach (var projectVisualBlock in projectVisualBlocks)
						{
							if (message.ApiEnviroment.UpdateInformation(blockInfo, message.Row, message.Item.Id, repeatBlock.Order,
								projectVisualBlock))
								successFlag = true;
						}
					}
					else
					{
						if (message.ApiEnviroment.AddInformation(blockInfo, message.Row, message.Item.Id, repeatBlock.Order))
							successFlag = true;
					}					
				}
			}
			return !successFlag ? null : message;
		};

		public Func<EngineMessage, EngineMessage> CreateProjectField = message =>
		{
			var projectField = (ProjectField) message.GetCreatable();
			if (projectField == null) return null;

			var result = ApiRouter.ProjectFields.Create(message.Authenticator, projectField);
			message.Item = result;
			return message;
		};

		public Func<EngineMessage, EngineMessage> SetClient = message =>
		{
			var project = ProjectRepository.GetDetailed<ProjectsApi>(message.Authenticator, message.GetValueFromRow("Case Name"));
			if (project == null) return null;

			if (project.Client == null)
			{
				var clientName = message.GetValueFromRow("Client");
				if (string.IsNullOrEmpty(clientName)) return null;

				try
				{
					var client = new Participant(clientName, ' ', ParticipantType.GetPersonType(message.Authenticator));
					project.Client = ParticipantsRepository.GetOrCreate<ParticipantsApi>(message.Authenticator, clientName, client);
				}
				catch (Exception)
				{
					return null;
				}
				project = ApiRouter.Projects.Put(message.Authenticator, project);
			}
			message.Item = project;

			return message;
		};

		public Func<EngineMessage, EngineMessage> CreateType = message =>
		{
			var typeName = message.GetValueFromRow("Name");
			if (string.IsNullOrEmpty(typeName)) return null;
			var abbreviation = message.GetValueFromRow("Abbreviation");
			if (string.IsNullOrEmpty(abbreviation)) return null;

			var processingType = ProjectTypeRepository.GetDetailedOrPut(message.Authenticator, typeName, abbreviation);		

			var blockName = message.GetValueFromRow("Blocks");
			var block = VisualBlockRepository.Get<VisualBlockApi>(message.Authenticator, blockName);
			if (block == null) return null;

			if (processingType.VisualBlocks == null) processingType.VisualBlocks = new List<VisualBlock>();
			if (processingType.VisualBlocks.Contains(block)) return null;

			processingType.VisualBlocks.Add(block);
			var updatedType = ApiRouter.ProjectTypes.Create(message.Authenticator, processingType);

			message.Item = updatedType;
			return message;
		};

		public Func<EngineMessage, EngineMessage> AttachParticipant = message =>
		{
			var projectName = message.GetValueFromRow("Case Name");
			var project = ProjectRepository.Get<ProjectsApi>(message.Authenticator, projectName);
			if (project == null) return null;

			var participantName = message.GetValueFromRow("Participant");
			var detailedParticipant =
				ParticipantsRepository.GetDetailed<ParticipantsApi>(message.Authenticator, participantName);
			if (detailedParticipant == null) return null;

			detailedParticipant.IncludeInProjectId = project.Id;

			ApiRouter.Participants.Create(message.Authenticator, detailedParticipant);
			message.Item = detailedParticipant;
			return message;
		};

		private static List<DictionaryItem> _calculationTypes;

		public Func<EngineMessage, EngineMessage> UpdateSettings = message =>
		{
			var projectName = message.GetValueFromRow("Case Name");
			var project = ProjectRepository.Get<ProjectsApi>(message.Authenticator, projectName);
			if (project == null) return null;

			var billingRules = (BillingRuleWrapper) message.GetCreatable(project);

			var projectSettings = ApiRouter.ProjectSettings.Get(message.Authenticator, project.Id);
			if (_calculationTypes == null)
				_calculationTypes = ApiRouter.DefaultDictionaryItems.GetMany(message.Authenticator, "CalculationTypes");

			projectSettings.BillingRules.AddRange(billingRules.BillingRules);
			ApiRouter.ProjectSettings.Save(message.Authenticator, projectSettings);

			message.Item = project;
			return message;
		};
	}
}
