using System;
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
				
				var apiEnviroment = new ApiEnviroment(authenticator);
				var counter = new Counter();
				var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = settings.MaxDegreeOfParallelism };
				Parallel.ForEach(excelTable, parallelOptions, (excelRow, state, index) =>
				{
					var request = new EngineRequest
					{								
						ApiEnviroment = apiEnviroment,
						ExcelRow = excelRow,
						BlockReader = blockReaderController,
						Settings = settings
					};
					var response = Processor.Invoke(request);
					if (response == null) return;

					counter.ProcessCount((int) index + settings.StartRow, excelTable.Count, response.Item, 70);
				});
			}
		}		
	}
}
