using System;
using System.Linq;
using PravoAdder.DatabaseEnviroment;
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

                Console.Write($"\tAdding project group {headerBlock.ProjectGroupName}...");
                var projectGroupSender = filler.AddProjectGroup(
                    projectGroupName: headerBlock.ProjectGroupName,
                    folderName: settings.FolderName,
                    description: "Created automatically.").Result;
                Console.WriteLine($"{projectGroupSender.Message}");

                Console.Write($"\tAdding project {headerBlock.ProjectName}...");
                var projectSender = filler.AddProject(settings, headerBlock, projectGroupSender.Content).Result;
                Console.WriteLine($"{projectSender.Message}");

                foreach (var blockInfo in blocksInfo)
                {
                    Console.WriteLine($"\tAdding information to project's block {blockInfo.Name}...");
                    filler.AddInformation(
                        projectId: projectSender.Content,
                        blockInfo: blockInfo,
                        excelRow: excelRow);
                }
                Console.WriteLine("\t...");
            }           
        }
    }
}
