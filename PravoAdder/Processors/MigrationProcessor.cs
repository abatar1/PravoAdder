using System;
using System.Threading.Tasks;
using PravoAdder.Wrappers;

namespace PravoAdder.Processors
{
	public class MigrationProcessor : IProcessor
	{
		public string ConfigFilename { get; }
		public Func<EngineRequest, EngineResponse> Processor { get; }		

		public MigrationProcessor()
		{
			
		}

		public MigrationProcessor(string configFilename, Func<EngineRequest, EngineResponse> processor)
		{
			ConfigFilename = configFilename;
			Processor = processor;
		}

		public void Run()
		{
			var settingsController = new SettingsWrapper();
			var settings = settingsController.LoadSettings(ConfigFilename);

			var authenticatorController = new AuthentificatorWrapper(settings);
			using (var authenticator = authenticatorController.Authenticate())
			{
				var blockReaderController = new BlockReaderWrapper(settings, authenticator);
				var excelTable = blockReaderController.Table.TableContent;

				var migrationProcessController = new DatabaseEnviromentWrapper(authenticator, settings);
				var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = settings.MaxDegreeOfParallelism };
				Parallel.ForEach(excelTable, parallelOptions, (excelRow, state, index) =>
				{
					var request = new EngineRequest
					{								
						Migrator = migrationProcessController,
						ExcelRow = excelRow,
						BlockReader = blockReaderController
					};
					var response = Processor.Invoke(request);
					if (response == null) return;

					migrationProcessController.ProcessCount((int) index + settings.StartRow, excelTable.Count, response.Item, 70);
				});
			}
		}		
	}
}
