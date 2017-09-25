using System;
using System.Threading.Tasks;
using PravoAdder.Controllers;

namespace PravoAdder
{
	public class ProjectProcessor : IProcessor
	{
		private readonly string _configFilename;
		public Func<EngineRequest, EngineResponse> Processor { get; }		

		public ProjectProcessor()
		{
			
		}

		public ProjectProcessor(string configFilename, Func<EngineRequest, EngineResponse> processor)
		{
			_configFilename = configFilename;
			Processor = processor;
		}

		public void Run()
		{
			var settingsController = new SettingsController();
			var settings = settingsController.LoadSettings(_configFilename);

			var authenticatorController = new AuthentificatorController(settings);
			using (var authenticator = authenticatorController.Authenticate())
			{
				var blockReaderController = new BlockReaderController(settings, authenticator);
				var excelTable = blockReaderController.Table.TableContent;

				var migrationProcessController = new MigrationProcessController(authenticator, settings);
				var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = settings.MaxDegreeOfParallelism };
				Parallel.ForEach(excelTable, parallelOptions, (excelRow, state, index) =>
				{
					var request = new EngineRequest(migrationProcessController, blockReaderController, excelRow);
					var response = Processor.Invoke(request);
					if (response == null) return;

					migrationProcessController.ProcessCount((int) index + settings.StartRow, excelTable.Count, response.Project, 70);
				});
			}
		}		
	}
}
