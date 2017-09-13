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
                var excelTable = blockReaderController.ExcelTable.TableContent;	

                var migrationProcessController = new MigrationProcessController(authenticator, settings);
                var parallelOptions = new ParallelOptions {MaxDegreeOfParallelism = settings.MaxDegreeOfParallelism};
                Parallel.ForEach(excelTable, parallelOptions, (excelRow, state, index) =>
                {
	                var headerBlock = blockReaderController.ReadHeader(excelRow);
	                if (headerBlock == null) return;

	                var projectGroupId = migrationProcessController.AddProjectGroup(headerBlock);

	                var projectId = migrationProcessController.AddProject(headerBlock, projectGroupId);
	                if (string.IsNullOrEmpty(projectId)) return;

	                var blocksInfo = blockReaderController.ReadBlockInfo();
	                foreach (var blockInfo in blocksInfo)
	                {
						migrationProcessController.AddInformationAsync(blockInfo, excelRow, projectId);
					}                       

	                if (string.IsNullOrEmpty(headerBlock.SynchronizationNumber))
	                {
						migrationProcessController.Synchronize(projectId, headerBlock.SynchronizationNumber);
					}					

	                migrationProcessController.ProcessCount((int)index + settings.StartRow, excelTable.Count,
		                headerBlock, projectId);
				});
            }
        }
    }
}