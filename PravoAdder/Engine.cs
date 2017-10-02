using System;
using System.Collections.Generic;
using System.Linq;
using Fclp;
using Fclp.Internals.Extensions;
using PravoAdder.Domain;
using PravoAdder.Processors;
using NLog;
using PravoAdder.Api;
using PravoAdder.Api.Domain;

namespace PravoAdder
{
	public class Engine
	{
		private ApplicationArguments _arguments;
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		public Engine Initialize(string[] args)
		{
			var parser = new FluentCommandLineParser<ApplicationArguments>();
		
			parser.Setup(arg => arg.ProcessType)
				.As('t', "type")
				.Required();

			parser.Setup(arg => arg.ConfigFilename)
				.As('c', "config")
				.SetDefault("config.json");

			var result = parser.Parse(args);
			_arguments = result.HasErrors == false ? parser.Object : null;

			return this;
		}

		public Engine Run()
		{
			
			var processor = CreateProcessor(_arguments);
			processor.Run();
			Logger.Info($"{DateTime.Now} | {_arguments.ProcessType} successfully processed. Press any key to continue.");
			Console.ReadKey();

			return this;
		}

		private static IProcessor CreateProcessor(ApplicationArguments arguments)
		{
			switch (arguments.ProcessType)
			{
				case ProcessType.Migration:
				{
					Console.Title = "Pravo.Migration";

					return new MigrationProcessor(arguments, request =>
					{
						var engineMessage = ProcessorImplementations.AddProjectProcessor(request);
						if (engineMessage == null) return null;

						var blocksInfo = request.BlockReader.Read();
						request.Row.Participants.ForEach(p => request.Migrator.AttachParticipant(p, engineMessage.Item.Id));
						foreach (var repeatBlock in blocksInfo)
						{
							foreach (var blockInfo in repeatBlock.Blocks)
							{
								request.Migrator.AddInformationAsync(blockInfo, request.Row, engineMessage.Item.Id, repeatBlock.Order);
							}
						}
						return engineMessage;
					});
				}
				case ProcessType.Syncronization:
				{
					Console.Title = "Pravo.Sync";

					return new MigrationProcessor(arguments, request =>
					{						
						var engineMessage = ProcessorImplementations.AddProjectProcessor(request);
						if (engineMessage == null) return null;

						if (string.IsNullOrEmpty(engineMessage.HeaderBlock.SynchronizationNumber))
						{
							request.Migrator.Synchronize(engineMessage.Item.Id, engineMessage.HeaderBlock.SynchronizationNumber);
						}

						return engineMessage;
					});
				}
				case ProcessType.CleanAll:
				{
					Console.Title = "Pravo.Clean";

					return new ForEachProjectGroupProcessor(arguments, request =>
					{
						var projectsResponse = (GroupedProjects) request.Migrator.GetGroupedProjects(request.Item.Id);
						var projects = new List<Project>();
						if (projectsResponse != null) projects = projectsResponse.Projects;

						foreach (var pair in projects.Select((project, count) => new {Project = project, Count = count}))
						{
							request.Migrator.DeleteProject(pair.Project.Id);
							request.Migrator.ProcessCount(pair.Count, 0, pair.Project, 70);
						}
						if (request.Item.Id != null)
						{
							request.Migrator.DeleteProjectGroup(request.Item.Id);
						}					

						var folders = request.Migrator.GetProjectFolders();
						foreach (var pair in folders.Select((f, c) => new { Folder = f, Count = c }))
						{
							if ((GroupedProjects) request.Migrator.GetGroupedProjects(null, pair.Folder.Name) != null)
								continue;
							request.Migrator.DeleteFolder(pair.Folder.Id);
							request.Migrator.ProcessCount(pair.Count, 0, pair.Folder, 70);
						}

						return new EngineResponse();
					});
				}				
				default:
					return null;
			}
		}
	}
}
