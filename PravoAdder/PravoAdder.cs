using System.Collections.Generic;
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
            var settings = ConsoleController.LoadSettings(_configFilename);

            var blocksInfo = ConsoleController.ReadBlockInfo(settings) as IList<BlockInfo> ?? new List<BlockInfo>();
            var excelTable = ConsoleController.ReadExcelFile(settings, new[] { "FF92D050", null });
            var authenticator = ConsoleController.ConsoleAutentification(settings);

            var filler = new DatabaseFiller(authenticator);

            foreach (var excelRow in excelTable)
            {
                var headerBlock = BlockInfoReader.ReadHeaderBlockInfo(settings.IdComparerPath, excelRow);
                var projectGroupId = ConsoleController.AddProjectGroup(headerBlock, filler, settings);
                var projectId = ConsoleController.AddProject(headerBlock, filler, settings, projectGroupId);

                foreach (var blockInfo in blocksInfo)
                {
                    ConsoleController.AddInformationAsync(blockInfo, filler, excelRow, projectId);
                }     
                ConsoleController.SplitLine();
            }
        }
    }
}