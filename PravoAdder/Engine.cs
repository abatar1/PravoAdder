using System;
using Fclp;
using PravoAdder.Domain;

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

					return new ProjectProcessor(arguments.ConfigFilename, request =>
					{
						var engineMessage = ProcessorImplementations.MigrationMasterDataProcessor(request);
						if (engineMessage == null) return null;

						var blocksInfo = request.BlockReader.ReadBlockInfo();
						foreach (var repeatBlock in blocksInfo)
						{
							foreach (var blockInfo in repeatBlock.Blocks)
							{
								request.Migrator.AddInformationAsync(blockInfo, request.ExcelRow, engineMessage.Project.Id, repeatBlock.Order);
							}
						}
						return engineMessage;
					});
				}
				case ProcessType.Syncronization:
				{
					Console.Title = "Pravo.Sync";

					return new ProjectProcessor(arguments.ConfigFilename, request =>
					{						
						var engineMessage = ProcessorImplementations.MigrationMasterDataProcessor(request);
						if (engineMessage == null) return null;

						if (string.IsNullOrEmpty(engineMessage.HeaderBlock.SynchronizationNumber))
						{
							request.Migrator.Synchronize(engineMessage.Project.Id, engineMessage.HeaderBlock.SynchronizationNumber);
						}

						return engineMessage;
					});
				}
				case ProcessType.Clearing:
				{
					Console.Title = "Pravo.Clean";

					return new CleanProcessor(request =>
					{
						return null;
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
