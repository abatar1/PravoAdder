using System;
using System.Threading.Tasks;
using PravoAdder.Domain;
using PravoAdder.Readers;
using PravoAdder.Wrappers;

namespace PravoAdder.Processors
{
	public class ParticipantProcessor : IProcessor
	{
		public ParticipantProcessor(ApplicationArguments applicationArguments, Func<EngineRequest, EngineResponse> processor)
		{
			ApplicationArguments = applicationArguments;
			Processor = processor;
		}

		public ApplicationArguments ApplicationArguments { get; }
		public Func<EngineRequest, EngineResponse> Processor { get; }

		public void Run()
		{
			var settingsController = new SettingsWrapper();
			var settings = settingsController.LoadSettingsFromConsole(ApplicationArguments);

			TableEnviroment.Initialize(ApplicationArguments, settings);
			var table = TableEnviroment.Table.TableContent;
			var info = TableEnviroment.Table.Header;

			var authenticatorController = new AuthentificatorWrapper(settings);
			using (var authenticator = authenticatorController.Authenticate())
			{
				var apiEnviroment = new ApiEnviroment(authenticator);
				var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = ApplicationArguments.MaxDegreeOfParallelism };
				var counter = new Counter();
				var participantConstructor = new ParticipantConstructor(authenticator);
				Parallel.ForEach(table, parallelOptions, (excelRow, state, index) =>
				{
					var request = new EngineRequest
					{
						ApiEnviroment = apiEnviroment,
						ExcelRow = excelRow,
						Participant = participantConstructor.Create(info, excelRow)
					};

					var response = Processor.Invoke(request);
					if (response == null) return;
					counter.ProcessCount((int)index + ApplicationArguments.RowNum, table.Count, response.Item, 70);
				});
			}
		}
	}
}
