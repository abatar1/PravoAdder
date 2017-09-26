using System;
using System.Linq;
using Fclp;
using PravoAdder.Domain;
using PravoAdder.Processors;

namespace PravoAdder
{
	public class Engine
	{
		private ApplicationArguments _arguments;

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

		private static IProcessor CreateProcessor(ApplicationArguments arguments)
		{
			switch (arguments.ProcessType)
			{
				case ProcessType.Migration:
				{
					Console.Title = "Pravo.Add";

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
				case ProcessType.Cleaning:
				{
					Console.Title = "Pravo.Clean";

					return new ForEachProjectGroupProcessor(arguments, request =>
					{
						var projects = request.Migrator.GetProjects(request.Item.Id);
						
						foreach (var project in projects.Select((project, count) => new {Content = project, Count = count}))
						{
							request.Migrator.DeleteProject(project.Content.Id);
							request.Migrator.ProcessCount(project.Count, 0, project.Content, 70);
						}
						request.Migrator.DeleteProjectGroup(request.Item.Id);

						var folders = request.Migrator.GetProjectFolders();
						foreach (var folder in folders.Select((folder, count) => new { Content = folder, Count = count }))
						{
							request.Migrator.DeleteFolder(folder.Content.Id);
							request.Migrator.ProcessCount(folder.Count, 0, folder.Content, 70);
						}

						return new EngineResponse();
					});
				}				
				default:
					return null;
			}
		}

		public Engine Run()
		{
			var processor = CreateProcessor(_arguments);
			processor.Run();
			return this;
		}
	}
}
