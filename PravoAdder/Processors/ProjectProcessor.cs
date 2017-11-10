using System;
using System.Collections.Generic;
using System.Linq;
using PravoAdder.Api.Domain;
using PravoAdder.Domain;

namespace PravoAdder.Processors
{
	public class ProjectProcessor
	{
		private static List<Project> _projects;
		private static List<ProjectType> _projectTypes;
		private static List<VisualBlock> _visualBlocks;

		public Func<EngineMessage, EngineMessage> Update = message =>
		{
			throw new NotImplementedException();
		};

		public Func<EngineMessage, EngineMessage> TryCreate = message =>
		{
			var headerBlock = message.CaseBuilder.ReadHeaderBlock(message.Row);
			if (headerBlock == null) return null;

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
			if (_projects == null) _projects = ApiRouter.Projects.GetMany(message.Authenticator);
			var project = _projects.FirstOrDefault(p => p.Name == projectName);
			if (project == null) return null;

			project.Name = Table.GetValue(message.Table.Header, message.Row, "New case name");
			message.Item = ApiRouter.Projects.Put(message.Authenticator, project);
			return message;
		};

		public Func<EngineMessage, EngineMessage> AddNote = message =>
		{
			var projectName = Table.GetValue(message.Table.Header, message.Row, "Case");

			if (_projects == null) _projects = ApiRouter.Projects.GetMany(message.Authenticator);
			var project = _projects.FirstOrDefault(p => projectName.Contains(p.Name));
			if (project == null) return null;

			var note = new Note
			{
				Project = project,
				Text = Table.GetValue(message.Table.Header, message.Row, "Notes")
			};
			ApiRouter.Notes.Create(message.Authenticator, note);
			return new EngineMessage { Item = project };
		};

		public Func<EngineMessage, EngineMessage> Synchronize = message =>
		{
			if (!string.IsNullOrEmpty(message.HeaderBlock.CasebookNumber))
			{
				if (_projects == null) _projects = ApiRouter.Projects.GetMany(message.Authenticator);
				var project = _projects.First(p => p.Name == message.HeaderBlock.Name);

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

			var blocksInfo = message.CaseBuilder.Build();
			foreach (var repeatBlock in blocksInfo)
			{
				foreach (var blockInfo in repeatBlock.VisualBlocks)
				{
					message.ApiEnviroment.AddInformation(blockInfo, message.Row, message.Item.Id, repeatBlock.Order);
				}
			}
			return message;
		};

		public static Func<EngineMessage, EngineMessage> CreateProjectField = message =>
		{
			var projectField = (ProjectField) message.GetCreatable();
			if (projectField == null) return null;

			var result = ApiRouter.ProjectFields.Create(message.Authenticator, projectField);
			message.Item = result;
			return message;
		};

		public Func<EngineMessage, EngineMessage> CreateType = message =>
		{
			if (_projectTypes == null)
			{
				_projectTypes = ApiRouter.ProjectTypes.GetMany(message.Authenticator)
					.Select(t => ApiRouter.ProjectTypes.Get(message.Authenticator, t.Id))
					.ToList();
			}

			var typeName = message.GetValueFromRow("Name");
			if (string.IsNullOrEmpty(typeName)) return null;
			var abbreviation = message.GetValueFromRow("Abbreviation");
			if (string.IsNullOrEmpty(abbreviation)) return null;

			var processingType = _projectTypes.FirstOrDefault(v => v.Name.Equals(typeName, StringComparison.InvariantCultureIgnoreCase));			
			if (processingType == null)
			{
				processingType = ApiRouter.ProjectTypes.PostWithBlocks(message.Authenticator, typeName, abbreviation);
				_projectTypes.Add(processingType);
			}				

			var blockName = message.GetValueFromRow("Blocks");
			if (_visualBlocks == null) _visualBlocks = ApiRouter.VisualBlocks.GetMany(message.Authenticator);
			var block = _visualBlocks.FirstOrDefault(b => b.Name.Equals(blockName, StringComparison.InvariantCultureIgnoreCase));
			if (block == null) return null;

			if (processingType.VisualBlocks == null) processingType.VisualBlocks = new List<VisualBlock>();
			if (processingType.VisualBlocks.Contains(block)) return null;

			processingType.VisualBlocks.Add(block);
			var updatedType = ApiRouter.ProjectTypes.PutWithBlocks(message.Authenticator, processingType);

			message.Item = updatedType;
			return message;
		};

		public Func<EngineMessage, EngineMessage> AttachParticipant = message =>
		{
			var projectName = Table.GetValue(message.Table.Header, message.Row, "Case name");
			if (_projects == null) _projects = ApiRouter.Projects.GetMany(message.Authenticator);
			var project = _projects.FirstOrDefault(p => p.Name == projectName);
			if (project == null) return null;

			var participantName = Table.GetValue(message.Table.Header, message.Row, "Participant");
			var detailedParticipant =
				ParticipantProcessor.GetParticipantByName(message.Authenticator, message.Table.Header, message.Row,
					participantName);
			if (detailedParticipant == null) return null;

			detailedParticipant.IncludeInProjectId = project.Id;

			ApiRouter.Participants.Put(message.Authenticator, detailedParticipant);
			message.Item = (Participant) detailedParticipant;
			return message;
		};
	}
}
