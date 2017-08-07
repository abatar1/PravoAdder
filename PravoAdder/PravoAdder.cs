﻿using System;
using System.Linq;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Helper;
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
            Console.WriteLine("Reading config files...");
            var settings = SettingsReader.Read(_configFilename);
            ConsoleHelper.LoadConfigFromConsole(settings);
            settings.Save(_configFilename);
            var blocksInfo = BlockReader.ReadBlocks(settings.IdComparerPath).ToList();                                

            var excelTable = ConsoleHelper.ReadExcelFile(settings.DataRowPosition, settings.InformationRowPosition);

            var filler = new DatabaseFiller();
            ConsoleHelper.Autentification(settings, filler);                      

            foreach (var excelRow in excelTable)
            {
                var headerBlock = BlockReader.ReadHeaderBlock(settings.IdComparerPath, excelRow);

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
                    var blockSender = filler.AddInformation(
                        projectId: projectSender.Content,
                        blockInfo: blockInfo,
                        excelRow: excelRow);
                }
                Console.WriteLine("\t...");
            }
        }
    }
}