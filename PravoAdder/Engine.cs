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

		public Engine Run()
		{
			var projectProcessor = new ProjectProcessor();

			if (_arguments.ProcessType == ProcessType.Migration)
			{
				Console.Title = "Pravo.Add";

				projectProcessor = new ProjectProcessor(_arguments.ConfigFilename, request =>
				{
					var engineMessage = ProjectProcessor.MigrationMasterDataProcessor(request);
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
			else if (_arguments.ProcessType == ProcessType.Syncronization)
			{
				Console.Title = "Pravo.Sync";

				projectProcessor = new ProjectProcessor(_arguments.ConfigFilename, request =>
				{
					var engineMessage = ProjectProcessor.MigrationMasterDataProcessor(request);
					if (engineMessage == null) return null;

					if (string.IsNullOrEmpty(engineMessage.HeaderBlock.SynchronizationNumber))
					{
						request.Migrator.Synchronize(engineMessage.Project.Id, engineMessage.HeaderBlock.SynchronizationNumber);
					}

					return engineMessage;
				});
			}
			projectProcessor.Run();

			return this;
		}
	}
}
