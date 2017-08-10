using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Domain;
using PravoAdder.Domain.Info;
using PravoAdder.Reader;

namespace PravoAdder
{
    public class ConsoleController
    {
        public static HttpAuthenticator Autentification(Settings settings)
        {
            while (true)
            {              
                Console.WriteLine($"Login as {settings.Login}...");
                try
                {
                    var authentication = new HttpAuthenticator(settings.BaseUri);
                    authentication.Authentication(settings.Login, settings.Password);
                    return authentication;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public static Settings LoadSettings(string configFilename)
        {
            Console.WriteLine("Reading config files...");
            var settings = SettingsReader.Read(configFilename);

            foreach (var property in settings.GetType().GetProperties())
            {
                var nameAttribute = (DisplayNameAttribute)property.GetCustomAttributes(typeof(DisplayNameAttribute)).FirstOrDefault();
                var displayName = nameAttribute != null ? nameAttribute.DisplayName : property.Name;
                var value = property.GetValue(settings);

                if (string.IsNullOrEmpty(value?.ToString()))
                {
                    property.SetValue(settings, LoadValue(displayName, property.PropertyType));
                }
            }

            settings.Save(configFilename);
            return settings;
        }

        private static object LoadValue(string message, Type type)
        {
            while (true)
            {
                Console.WriteLine($"{message}: ");
                var data = Console.ReadLine();
                if (!string.IsNullOrEmpty(data)) return Convert.ChangeType(data, type);
                Console.WriteLine($"Wrong {message}!");
            }
        }      

        public static IEnumerable<BlockInfo> ReadBlockInfo(Settings settings)
        {
            Console.WriteLine("Reading block info file...");
            return BlockInfoReader.ReadBlocksInfo(settings.IdComparerPath).ToList();
        }

        public static IEnumerable<IDictionary<int, string>> ReadExcelFile(int dataRowPosition, int infoRowPosition, string[] allowedColors)
        {
            var state = true;
            var excelFilename = "";
            while (true)
            {                
                if (state)
                {
                    Console.WriteLine("Write excel filename: ");
                    var filename = Console.ReadLine();
                    if (string.IsNullOrEmpty(filename)) continue;
                    excelFilename = filename.Contains(".xlsx") ? filename : $"{filename}.xlsx";
                }
                
                Console.WriteLine("Reading excel file...");
                try
                {   
                    return ExcelReader.ReadDataFromTable(excelFilename, dataRowPosition, infoRowPosition, allowedColors).ToList();
                }
                catch (FileNotFoundException ex)
                {
                    Console.WriteLine($"{ex.Message}");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"{ex.Message} Please close file and press Enter.");
                    state = false;
                    Console.ReadLine();
                }                
            }
        }

        public static string AddProjectGroup(HeaderBlockInfo headerBlock, DatabaseFiller filler, Settings settings)
        {
            Console.Write($"\tAdding project group {headerBlock.ProjectGroupName}...");
            var projectGroupSender = filler.AddProjectGroupAsync(
                projectGroupName: headerBlock.ProjectGroupName,
                folderName: settings.FolderName,
                description: headerBlock.Description).Result;
            Console.WriteLine($"{projectGroupSender.Message}");

            return projectGroupSender.Content;
        }

        public static string AddProject(HeaderBlockInfo headerBlock, DatabaseFiller filler, Settings settings, string projectGroupId)
        {
            Console.Write($"\tAdding project {headerBlock.ProjectName}...");
            var projectSender = filler.AddProjectAsync(settings, headerBlock, projectGroupId).Result;
            Console.WriteLine($"{projectSender.Message}");

            return projectSender.Content;
        }

        public static async void AddInformationAsync(BlockInfo blockInfo, DatabaseFiller filler, IDictionary<int, string> excelRow, string projectId)
        {
            Console.WriteLine($"\tAdding information to project's block {blockInfo.Name}...");
            var blockSender = await filler.AddInformationAsync(
                projectId: projectId,
                blockInfo: blockInfo,
                excelRow: excelRow);
        }

        public static void SplitLine()
        {
            Console.WriteLine("\t...");
        }
    }
}
