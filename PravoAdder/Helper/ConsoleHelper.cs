using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Domain;
using PravoAdder.Reader;

namespace PravoAdder.Helper
{
    public class ConsoleHelper
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

        private static T LoadValueFromConsole<T>(string message)
        {
            while (true)
            {
                Console.WriteLine($"{message}: ");
                var data = Console.ReadLine();
                if (string.IsNullOrEmpty(data))
                {
                    Console.WriteLine($"Wrong {message}!");
                    continue;
                }
                return (T)Convert.ChangeType(data, typeof(T));
            }
        }

        public static void LoadConfigFromConsole(Settings settings)
        {
            if (string.IsNullOrEmpty(settings.Login))
            {
                settings.Login = LoadValueFromConsole<string>("Login");
            }

            if (string.IsNullOrEmpty(settings.FolderName))
            {
                settings.FolderName = LoadValueFromConsole<string>("Folder name");
            }

            if (string.IsNullOrEmpty(settings.ProjectTypeName))
            {
                settings.ProjectTypeName = LoadValueFromConsole<string>("Project type name");
            }

            if (settings.DataRowPosition == 0)
            {
                settings.DataRowPosition = LoadValueFromConsole<int>("Data row position");
            }

            if (settings.InformationRowPosition == 0)
            {
                settings.InformationRowPosition = LoadValueFromConsole<int>("Information row position");
            }

            if (string.IsNullOrEmpty(settings.IdComparerPath))
            {
                settings.IdComparerPath = LoadValueFromConsole<string>("Id comparer path");
            }
        }

        public static IEnumerable<IDictionary<int, string>> ReadExcelFile(int dataRowPosition, int infoRowPosition)
        {
            var state = true;
            var excelFilename = "";
            while (true)
            {                
                if (state)
                {
                    Console.WriteLine("Write excel filename: ");
                    var filename = Console.ReadLine();
                    if (filename == null) continue;
                    excelFilename = filename.Contains(".xlsx") ? filename : $"{filename}.xlsx";
                }
                
                Console.WriteLine("Reading excel file...");
                try
                {   
                    return ExcelReader.ReadDataFromTable(excelFilename, dataRowPosition, infoRowPosition).ToList();
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
    }
}
