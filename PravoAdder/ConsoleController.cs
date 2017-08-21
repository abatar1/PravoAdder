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
	    public ConsoleController(TextWriter writer)
	    {
		    Console.SetOut(writer);
	    }

        public HttpAuthenticator Authenticate(Settings settings)
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

        public Settings LoadSettings(string configFilename)
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

        public IList<BlockInfo> ReadBlockInfo(Settings settings)
        {
            Console.WriteLine("Reading block info file...");
            return BlockInfoReader.Read(settings.IdComparerPath) as List<BlockInfo> ?? new List<BlockInfo>();
        }

        public IEnumerable<IDictionary<int, string>> ReadExcelFile(Settings settings, string[] allowedColors)
        {
			Console.WriteLine("Reading excel file...");
			return ExcelReader.Read(settings.ExcelFileName, settings.DataRowPosition, settings.InformationRowPosition, allowedColors).ToList();
        }

        public string AddProjectGroup(HeaderBlockInfo headerBlock, DatabaseFiller filler, Settings settings)
        {
            Console.Write($"\tAdding project group {headerBlock.ProjectGroupName}...");
            var projectGroupSender = filler.AddProjectGroupAsync(settings, headerBlock).Result;
            Console.WriteLine($"{projectGroupSender.Message}");

            return projectGroupSender.Content;
        }

        public string AddProject(HeaderBlockInfo headerBlock, DatabaseFiller filler, Settings settings, string projectGroupId)
        {
            Console.Write($"\tAdding project {headerBlock.ProjectName}...");
            var projectSender = filler.AddProjectAsync(settings, headerBlock, projectGroupId).Result;
            Console.WriteLine($"{projectSender.Message}");

            return projectSender.Content;
        }

        public void AddInformationAsync(BlockInfo blockInfo, DatabaseFiller filler, IDictionary<int, string> excelRow, string projectId)
        {
            Console.WriteLine($"\tAdding information to project's block {blockInfo.Name}...");
            var blockSender = filler.AddInformationAsync(
                projectId: projectId,
                blockInfo: blockInfo,
                excelRow: excelRow);
        }

        public void SplitLine()
        {
            Console.WriteLine("\t...");
        }
    }
}
