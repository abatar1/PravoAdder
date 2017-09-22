using System;
using System.Threading.Tasks;
using PravoAdder.Api.Domain;
using PravoAdder.Controllers;

namespace PravoAdder
{
	public class ProjectProcessor
	{
		private readonly string _configFilename;
		private readonly Func<EngineRequest, EngineResponse> _processor;
		public static Func<EngineRequest, EngineResponse> MigrationMasterDataProcessor = request =>
		{
			var headerBlock = request.BlockReader.ReadHeader(request.ExcelRow);
			if (headerBlock == null) return null;

			var projectGroup = request.Migrator.AddProjectGroup(headerBlock);
			var project = request.Migrator.AddProject(headerBlock, projectGroup?.Id);

			return string.IsNullOrEmpty(project?.Id) ? null : new EngineResponse(headerBlock, (Project)project);
		};

		public ProjectProcessor()
		{
			
		}

		public ProjectProcessor(string configFilename, Func<EngineRequest, EngineResponse> processor)
		{
			_configFilename = configFilename;
			_processor = processor;
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
					var response = _processor.Invoke(request);
					if (response == null) return;

					migrationProcessController.ProcessCount((int) index + settings.StartRow, excelTable.Count, response.Project, 70);
				});
			}
		}		
	}
}
