﻿using System;
using System.Threading.Tasks;
using PravoAdder.Wrappers;
using PravoAdder.Domain;

namespace PravoAdder.Processors
{
	public class MigrationProcessor : IProcessor
	{
		public Func<EngineRequest, EngineResponse> Processor { get; }
		public ApplicationArguments ApplicationArguments { get; }

		public MigrationProcessor()
		{
			
		}

		public MigrationProcessor(ApplicationArguments arguments, Func<EngineRequest, EngineResponse> processor)
		{
			ApplicationArguments = arguments;
			Processor = processor;
		}

		public void Run()
		{
			var settingsController = new SettingsWrapper();
			var settings = settingsController.LoadSettingsFromConsole(ApplicationArguments);

			var authenticatorController = new AuthentificatorWrapper(settings);
			using (var authenticator = authenticatorController.Authenticate())
			{
				var blockReaderController = new BlockReaderWrapper(settings, authenticator);
				var excelTable = blockReaderController.Table.TableContent;
				
				var migrationProcessController = new DatabaseEnviromentWrapper(authenticator, settings);
				var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = settings.MaxDegreeOfParallelism };
				var count = 0;
				Parallel.ForEach(excelTable, parallelOptions, (excelRow, state, index) =>
				{
					if (count > settings.MaximumRows) state.Break();

					var request = new EngineRequest
					{								
						Migrator = migrationProcessController,
						ExcelRow = excelRow,
						BlockReader = blockReaderController
					};
					var response = Processor.Invoke(request);
					if (response == null) return;

					count += 1;
					migrationProcessController.ProcessCount((int) index + settings.StartRow, excelTable.Count, response.Item, 70);
				});
			}
		}		
	}
}
