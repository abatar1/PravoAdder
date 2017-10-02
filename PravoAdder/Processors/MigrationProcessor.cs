using System;
using System.Threading.Tasks;
using PravoAdder.Wrappers;
using PravoAdder.Domain;
using PravoAdder.TableEnviroment;

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
			var settingsLoader = new SettingsLoader();
			var settings = settingsLoader.LoadSettingsFromConsole(ApplicationArguments);

			TablesContainer.Initialize(new TableSettings(settings));
			var table = TablesContainer.Table.TableContent;

			var authenticatorController = new AuthentificatorWrapper(settings);
			using (var authenticator = authenticatorController.Authenticate())
			{				
				var blockReader = new BlockReader(authenticator);				
				var databaseWrapper = new DatabaseEnviromentWrapper(authenticator, settings);
				var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = settings.MaxDegreeOfParallelism };
				Parallel.ForEach(table, parallelOptions, (excelRow, state, index) =>
				{
					var request = new EngineRequest
					{								
						Migrator = databaseWrapper,
						Row = excelRow,
						BlockReader = blockReader
					};
					var response = Processor.Invoke(request);
					if (response == null) return;

					databaseWrapper.ProcessCount((int) index + settings.StartRow, table.Count, response.Item, 70);
				});
			}
		}		
	}
}
