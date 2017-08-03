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
            Console.WriteLine("Reading excel file...");
            var excel = ExcelReader.ReadDataFromTable("test.xlsx").ToList();

            Console.WriteLine("Reading config files...");
            var blocksInfo = BlockReader.ReadBlocks("blocksInfo.json").ToList();
            var settings = SettingsReader.Read("config.json");

            var filler = new DatabaseFiller();           

            Console.WriteLine($"Login as {settings.Login}...");
            filler.Authentication(
                login: settings.Login,
                password: "123123");

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
