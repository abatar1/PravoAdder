using System.Threading.Tasks;
using PravoAdder.Controllers;
using PravoAdder.DatabaseEnviroment;

namespace PravoAdder
{
    public class PravoAdder
    {
        private readonly string _configFilename;

        private PravoAdder(string configFilename)
        {
            _configFilename = configFilename;
        }

	    public static PravoAdder Create(string configFilename)
	    {
		    return new PravoAdder(configFilename);
	    }

        public void Run()
        {
			var settingsController = new SettingsController();
			var settings = settingsController.LoadSettings(_configFilename);						
		
			var processController = new MigrationProcessController();
			using (var authenticator = processController.Authenticate(settings))
	        {
		        var blockReaderController = new BlockReaderController(settings, authenticator);
		        var blocksInfo = blockReaderController.ReadBlockInfo();
		        var excelTable = blockReaderController.ExcelTable.TableContent;

				var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = settings.MaxDegreeOfParallelism };
		        var filler = new DatabaseFiller(authenticator);
				Parallel.ForEach(excelTable, parallelOptions, (excelRow, state, index) =>
		        {
			        var headerBlock = blockReaderController.ReadHeader(excelRow);			        
					var projectGroupId = processController.AddProjectGroup(headerBlock, filler, settings);
			        var projectId = processController.AddProject(headerBlock, filler, settings, projectGroupId);

			        processController.ProcessCount((int)index + 1, excelTable.Count, headerBlock, projectId);
					Parallel.ForEach(blocksInfo, parallelOptions, blockInfo => processController.AddInformationAsync(blockInfo, filler, excelRow, projectId));
		        });
			}				
        }
    }
}