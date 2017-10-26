﻿using System;
using System.Collections.Generic;
using System.Linq;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Domain;

namespace PravoAdder.Processors
{
	public class ProjectProcessor
	{
		private static List<Project> _projects;

		public Func<EngineMessage, EngineMessage> Update = message =>
		{
			throw new NotImplementedException();
		};

		public Func<EngineMessage, EngineMessage> TryCreate = message =>
		{
			var headerBlock = message.CaseCreator.ReadHeaderBlock(message.Row);
			if (headerBlock == null) return null;

			var projectGroup = message.ApiEnviroment.AddProjectGroup(message.ApplicationArguments.IsOverwrite, headerBlock);
			var project = message.ApiEnviroment.AddProject(message.ApplicationArguments.IsOverwrite, headerBlock,
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
			if (_projects == null) _projects = ApiRouter.Projects.GetProjects(message.Authenticator);
			var project = _projects.FirstOrDefault(p => p.Name == projectName);
			if (project == null) return null;

			project.Name = Table.GetValue(message.Table.Header, message.Row, "New case name");
			message.Item = ApiRouter.Projects.PutProject(message.Authenticator, project);
			return message;
		};

		public Func<EngineMessage, EngineMessage> AddNote = message =>
		{
			var projectName = Table.GetValue(message.Table.Header, message.Row, "Case");

			if (_projects == null) _projects = ApiRouter.Projects.GetProjects(message.Authenticator);
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

		public Func<EngineMessage, EngineMessage> AddInformation = message =>
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
	}
}
