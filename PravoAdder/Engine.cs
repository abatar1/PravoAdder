using System;
using System.Linq;
using Fclp;
using PravoAdder.Domain;
using PravoAdder.Processors;
using NLog;
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

						var blocksInfo = request.BlockReader.ReadBlockInfo();
						foreach (var repeatBlock in blocksInfo)
						{
							foreach (var blockInfo in repeatBlock.Blocks)
							{
								request.Migrator.AddInformationAsync(blockInfo, request.ExcelRow, engineMessage.Item.Id, repeatBlock.Order);
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
						var projects = ((GroupedProjects) request.Migrator.GetGroupedProjects(request.Item.Id)).Projects;
						
						foreach (var p in projects.Select((project, count) => new {Project = project, Count = count}))
						{
							request.Migrator.DeleteProject(p.Project.Id);
							request.Migrator.ProcessCount(p.Count, 0, p.Project, 70);
						}
						request.Migrator.DeleteProjectGroup(request.Item.Id);

						var folders = request.Migrator.GetProjectFolders();
						foreach (var f in folders.Select((folder, count) => new { Folder = folder, Count = count }))
						{
							if ((GroupedProjects) request.Migrator.GetGroupedProjects(null, f.Folder.Name) != null)
								continue;
							request.Migrator.DeleteFolder(f.Folder.Id);
							request.Migrator.ProcessCount(f.Count, 0, f.Folder, 70);
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
