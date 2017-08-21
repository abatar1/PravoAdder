using System;
using System.Collections.Generic;
using System.Linq;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Domain.Info;
using PravoAdder.Reader;

namespace PravoAdder
{
    public class PravoAdder
    {
        private readonly string _configFilename;

        public PravoAdder(string configFilename)
        {
            _configFilename = configFilename;
        }

        public void Run()
        {           
			var consoleController = new ConsoleController(Console.Out);	      
			
            var settings = consoleController.LoadSettings(_configFilename);
            var blocksInfo = consoleController.ReadBlockInfo(settings);
            var excelTable = consoleController.ReadExcelFile(settings, new[] { "FF92D050", null });
            var authenticator = consoleController.Authenticate(settings);

            var filler = new DatabaseFiller(authenticator);

            foreach (var excelRow in excelTable)
            {
                var headerBlock = BlockInfoReader.ReadHeader(settings.IdComparerPath, excelRow);
                var projectGroupId = consoleController.AddProjectGroup(headerBlock, filler, settings);
                var projectId = consoleController.AddProject(headerBlock, filler, settings, projectGroupId);

                foreach (var blockInfo in blocksInfo)
                {
                    consoleController.AddInformationAsync(blockInfo, filler, excelRow, projectId);
                }     
                consoleController.SplitLine();
            }
        }
    }
}