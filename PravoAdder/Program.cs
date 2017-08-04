using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Domain;
using PravoAdder.Reader;

namespace PravoAdder
{
    internal class Program
    {      
        private static void Main(string[] args)
        {
            var excel = ConsoleHelper.ReadExcelFile();

            Console.WriteLine("Reading config files...");
            var blocksInfo = BlockReader.ReadBlocks("blocksInfo.json").ToList();
            var settings = SettingsReader.Read("config.json");

            var filler = new DatabaseFiller();
            ConsoleHelper.Autentification(settings, filler);
            ConsoleHelper.LoadConfigFromConsole(settings);
            settings.Save("config.json");

            foreach (var excelRow in excel)
            {
                var headerBlock = BlockReader.ReadHeaderBlock("blocksInfo.json", excelRow);
                Console.WriteLine($"\tAdding project group {headerBlock.ProjectGroupName}...");
                filler.AddProjectGroup(
                    projectGroupName: headerBlock.ProjectGroupName,
                    folderName: settings.FolderName);

                Console.WriteLine($"\tAdding project {headerBlock.ProjectName}...");
                var projectId = filler.AddProject(
                    projectName: headerBlock.ProjectName,
                    folderName: settings.FolderName,
                    projectTypeName: settings.ProjectTypeName,
                    responsibleName: headerBlock.ResponsibleName,
                    projectGroupName: headerBlock.ProjectGroupName);

                foreach (var blockInfo in blocksInfo)
                {
                    Console.WriteLine($"\tAdding information to project's block {blockInfo.Name}...");
                    filler.AddInformation(
                        projectId: projectId,
                        blockInfo: blockInfo,
                        excelRow: excelRow);
                }
                Console.WriteLine("\t...");
            }           
        }
    }
}
