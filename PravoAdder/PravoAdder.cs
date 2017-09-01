using System.Threading.Tasks;
using PravoAdder.Controllers;

namespace PravoAdder
{
	public class PravoAdder : IEngine
	{
		private readonly string _configFilename;

		public PravoAdder(string configFilename)
		{
			_configFilename = configFilename;
		}

		public void Run()
		{
			var settingsController = new SettingsController();
			var settings = settingsController.LoadSettings(_configFilename);

			var authenticatorController = new AuthentificatorController(settings);
			using (var authenticator = authenticatorController.Authenticate())
			{				
				var blockReaderController = new BlockReaderController(settings, authenticator);
				var blocksInfo = blockReaderController.ReadBlockInfo();
				var excelTable = blockReaderController.ExcelTable.TableContent;

				var migrationProcessController = new MigrationProcessController(authenticator, settings);
				var parallelOptions = new ParallelOptions {MaxDegreeOfParallelism = settings.MaxDegreeOfParallelism};
				Parallel.ForEach(excelTable, parallelOptions, (excelRow, state, index) =>
				{
					var headerBlock = blockReaderController.ReadHeader(excelRow);
					var projectGroupId = migrationProcessController.AddProjectGroup(headerBlock);
					var projectId = migrationProcessController.AddProject(headerBlock, projectGroupId);
					
					migrationProcessController.ProcessCount((int) index + settings.StartRow, excelTable.Count, headerBlock, projectId);

					foreach (var blockInfo in blocksInfo)
					{
						migrationProcessController.AddInformationAsync(blockInfo, excelRow, projectId);
					}					
				});
			}
		}
	}
}