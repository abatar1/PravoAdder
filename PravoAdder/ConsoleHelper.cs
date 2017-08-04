﻿using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Domain;
using PravoAdder.Reader;

namespace PravoAdder
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
            var state = true;
            var excelFilename = "";
            while (true)
            {                
                if (state)
                {
                    Console.WriteLine("Write excel filename: ");
                    excelFilename = $"{Console.ReadLine()}.xlsx";
                }
                
                Console.WriteLine("Reading excel file...");
                try
                {   
                    return ExcelReader.ReadDataFromTable(excelFilename).ToList();
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
