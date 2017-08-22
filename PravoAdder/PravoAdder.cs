using System;
using System.Linq;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Reader;

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
			var consoleController = new WriterController(Console.Out);	      
			
            var settings = consoleController.LoadSettings(_configFilename);
            var blocksInfo = consoleController.ReadBlockInfo(settings);
            var excelTable = consoleController.ReadExcelFile(settings, new[] { "FF92D050", null });
            var authenticator = consoleController.Authenticate(settings);

            var filler = new DatabaseFiller(authenticator);

            foreach (var excelContainer in excelTable.Select((row, count) => new {Row = row, Count = count}))
            {
				consoleController.ProcessCount(excelContainer.Count, excelTable.Count - 1);
                var headerBlock = BlockInfoReader.ReadHeader(settings.IdComparerPath, excelContainer.Row);
                var projectGroupId = consoleController.AddProjectGroup(headerBlock, filler, settings);
                var projectId = consoleController.AddProject(headerBlock, filler, settings, projectGroupId);

                foreach (var blockInfo in blocksInfo)
                {
                    consoleController.AddInformationAsync(blockInfo, filler, excelContainer.Row, projectId);
                }     
                consoleController.SplitLine();
            }
        }
    }
}