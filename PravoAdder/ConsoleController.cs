using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Domain;
using PravoAdder.Domain.Info;
using PravoAdder.Reader;

namespace PravoAdder
{
    public class ConsoleController
    {
        public static HttpAuthenticator ConsoleAutentification(Settings settings)
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
#if DEBUG
	            switch (property.Name)
	            {
		            case "Password":
			            property.SetValue(settings, "123123");
			            continue;
		            case "Overwrite":
			            property.SetValue(settings, false);
			            continue;
		            case "ExcelFileName":
			            property.SetValue(settings, "test2.xlsx");
			            continue;
		            case "DataRowPosition":
			            property.SetValue(settings, 9);
			            continue;
				}
#endif
				var nameAttribute = (DisplayNameAttribute) property.GetCustomAttributes(typeof(DisplayNameAttribute)).FirstOrDefault();
                var displayName = nameAttribute != null ? nameAttribute.DisplayName : property.Name;
                var value = property.GetValue(settings);

				if (string.IsNullOrEmpty(value?.ToString()) || property.PropertyType == typeof(bool))
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

        public static IEnumerable<IDictionary<int, string>> ReadExcelFile(Settings settings, string[] allowedColors)
        {
			Console.WriteLine("Reading excel file...");
			return ExcelReader.ReadDataFromTable(settings.ExcelFileName, settings.DataRowPosition, settings.InformationRowPosition, allowedColors).ToList();
        }

        public static string AddProjectGroup(HeaderBlockInfo headerBlock, DatabaseFiller filler, Settings settings)
        {
            Console.Write($"\tAdding project group {headerBlock.ProjectGroupName}...");
            var projectGroupSender = filler.AddProjectGroupAsync(
                projectGroupName: headerBlock.ProjectGroupName,
                folderName: settings.FolderName,
                description: headerBlock.Description,
				overwrite: settings.Overwrite).Result;
            Console.WriteLine($"{projectGroupSender.Message}");

            return projectGroupSender.Content;
        }

        public static string AddProject(HeaderBlockInfo headerBlock, DatabaseFiller filler, Settings settings, string projectGroupId)
        {
            Console.Write($"\tAdding project {headerBlock.ProjectName}...");
            var projectSender = filler.AddProjectAsync(settings, headerBlock, projectGroupId, settings.Overwrite).Result;
            Console.WriteLine($"{projectSender.Message}");

            return projectSender.Content;
        }

        public static void AddInformationAsync(BlockInfo blockInfo, DatabaseFiller filler, IDictionary<int, string> excelRow, string projectId)
        {
            Console.WriteLine($"\tAdding information to project's block {blockInfo.Name}...");
            var blockSender = filler.AddInformationAsync(
                projectId: projectId,
                blockInfo: blockInfo,
                excelRow: excelRow).Result;
        }

        public static void SplitLine()
        {
            Console.WriteLine("\t...");
        }
    }
}
