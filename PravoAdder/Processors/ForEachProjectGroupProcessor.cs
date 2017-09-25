using System;
using System.Threading.Tasks;
using PravoAdder.Wrappers;

namespace PravoAdder.Processors
{
	public class ForEachProjectGroupProcessor : IProcessor
	{
		public ForEachProjectGroupProcessor(string configFilename, Func<EngineRequest, EngineResponse> processor)
		{
			Processor = processor;
			ConfigFilename = configFilename;
		}

		public string ConfigFilename { get; }
		public Func<EngineRequest, EngineResponse> Processor { get; }

		public void Run()
		{
			var settingsController = new SettingsWrapper();
			var settings = settingsController.LoadSettings(ConfigFilename);

			var authenticatorController = new AuthentificatorWrapper(settings);
			using (var authenticator = authenticatorController.Authenticate())
			{
				var migrationProcessController = new DatabaseEnviromentWrapper(authenticator, settings);
				var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = settings.MaxDegreeOfParallelism };

				var projectGroups = migrationProcessController.GetProjectGroups();
				Parallel.ForEach(projectGroups, parallelOptions, (projectGroup, state, index) =>
				{
					var request = new EngineRequest
					{
						Migrator = migrationProcessController,
						Item = projectGroup
					};
					var response = Processor.Invoke(request);
					if (response == null) return;

					migrationProcessController.ProcessCount((int)index + settings.StartRow, 0, projectGroup, 70);
				});
			}
		}
	}
}
