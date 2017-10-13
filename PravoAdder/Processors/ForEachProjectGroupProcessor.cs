using System;
using System.Threading.Tasks;
using PravoAdder.Api.Domain;
using PravoAdder.Domain;
using PravoAdder.Wrappers;

namespace PravoAdder.Processors
{
	public class ForEachProjectGroupProcessor : IProcessor
	{
		public ForEachProjectGroupProcessor(ApplicationArguments applicationArguments, Func<EngineRequest, EngineResponse> processor)
		{
			Processor = processor;
			ApplicationArguments = applicationArguments;
		}

		public ApplicationArguments ApplicationArguments { get; }
		public Func<EngineRequest, EngineResponse> Processor { get; }

		public void Run()
		{
			var settingsController = new SettingsWrapper();
			var settings = settingsController.LoadSettingsFromConsole(ApplicationArguments);

			var authenticatorController = new AuthentificatorWrapper(settings);
			using (var authenticator = authenticatorController.Authenticate())
			{
				var migrationProcessController = new ApiEnviroment(authenticator);
				var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = ApplicationArguments.MaxDegreeOfParallelism };

				var projectGroups = migrationProcessController.GetProjectGroupItems();
				projectGroups.Add(ProjectGroup.Empty);
				Parallel.ForEach(projectGroups, parallelOptions, (projectGroup, state, index) =>
				{
					DateTime.TryParse(settings.DateTime, out var date);
					var request = new EngineRequest
					{
						ApiEnviroment = migrationProcessController,
						Item = projectGroup,
						Date = date,
						Settings = settings,
						AppArgs = ApplicationArguments
					};
					Processor.Invoke(request);					
				});
			}
		}
	}
}
