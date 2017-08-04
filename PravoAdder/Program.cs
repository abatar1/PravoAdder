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
        public static void Autentification(Settings settings, DatabaseFiller filler)
        {
            while (true)
            {
                if (settings.Login == string.Empty)
                {
                    Console.WriteLine("Login: ");
                    settings.Login = Console.ReadLine();
                }

                Console.WriteLine("Password: ");
                settings.Password = Console.ReadLine();

                Console.WriteLine($"Login as {settings.Login}...");
                try
                {
                    var authenticationStatus = filler.Authentication(
                        login: settings.Login,
                        password: settings.Password);
                    if (authenticationStatus) break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }                
            }           
        }

        public static void LoadConfigFromConsole(Settings settings)
        {
            if (settings.FolderName == string.Empty)
            {
                Console.WriteLine("Folder name: ");
                settings.FolderName = Console.ReadLine();
            }

            if (settings.ProjectTypeName == string.Empty)
            {
                Console.WriteLine("Project type name: ");
                settings.ProjectTypeName = Console.ReadLine();
            }
        }

        public static IEnumerable<IDictionary<int, string>> ReadExcelFile()
        {
            string excelFilename;
            while (true)
            {
                try
                {
                    Console.WriteLine("Write excel filename: ");
                    excelFilename = $"{Console.ReadLine()}.xlsx";
                    break;
                }
                catch (FileNotFoundException ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
            }

            while (true)
            {
                try
                {
                    Console.WriteLine("Reading excel file...");
                    return ExcelReader.ReadDataFromTable(excelFilename).ToList();
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"{ex.Message} Please close file and press Enter.");
                    Console.ReadLine();
                }
            }         
        }

        private static void Main(string[] args)
        {
            var excel = ReadExcelFile();

            Console.WriteLine("Reading config files...");
            var blocksInfo = BlockReader.ReadBlocks("blocksInfo.json").ToList();
            var settings = SettingsReader.Read("config.json");

            var filler = new DatabaseFiller();
            Autentification(settings, filler);
            LoadConfigFromConsole(settings);
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
