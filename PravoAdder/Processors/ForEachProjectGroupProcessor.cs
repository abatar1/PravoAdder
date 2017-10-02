﻿using System;
using System.Collections.Generic;
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
			var settingsController = new SettingsLoader();
			var settings = settingsController.LoadSettingsFromConsole(ApplicationArguments);

			var authenticatorController = new AuthentificatorWrapper(settings);
			using (var authenticator = authenticatorController.Authenticate())
			{
				var migrationProcessController = new DatabaseEnviromentWrapper(authenticator, settings);
				var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = settings.MaxDegreeOfParallelism };

				var projectGroups = migrationProcessController.GetProjectGroups();
				if (projectGroups == null || projectGroups.Count == 0)
				{
					projectGroups = new List<DatabaseEntityItem> { ProjectGroup.EmptyProjectGroup };
				}
				                    
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
