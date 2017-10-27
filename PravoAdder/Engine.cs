using System;
using System.Linq;
using Fclp;
using NLog;
using PravoAdder.Domain;
using PravoAdder.Processors;

namespace PravoAdder
{
	public class Engine
	{
		private static ApplicationArguments _arguments;
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		private Engine(ApplicationArguments arguments)
		{
			_arguments = arguments;
		}

		public Engine()
		{
			
		}

		public static readonly ProcessType[] TableProcesses =
		{
			ProcessType.Migration, ProcessType.Sync, ProcessType.CreateParticipants, ProcessType.Analyze, ProcessType.Notes,
			ProcessType.EditParticipantsByKey, ProcessType.RenameCases, ProcessType.AttachParticipant,
			ProcessType.EditParticipants,
		};

		public Engine Initialize(string[] args)
		{
			if (args.Length == 0) throw new ArgumentException();

			var parser = new FluentCommandLineParser<ApplicationArguments>();
		
			parser.Setup(arg => arg.ProcessType)
				.As('t', "type")
				.Required();
			parser.Setup(arg => arg.Password)
				.As('p', "password")
				.Required();
			parser.Setup(arg => arg.ParallelOptions)
				.As('a', "async")
				.SetDefault(1);
			parser.Setup(arg => arg.ConfigFileName)
				.As('c', "config")
				.SetDefault("config.json");
			parser.Parse(args);

			var processType = parser.Object.ProcessType;
			if (TableProcesses.Contains(processType))
			{
				parser.Setup(arg => arg.SourceFileName)
					.As('s', "sourcefile")
					.Required();
				parser.Setup(arg => arg.ReaderMode)
					.As('m', "mode")
					.Required();
				parser.Setup(arg => arg.RowNum)
					.As('r', "row")
					.SetDefault(1);
				parser.Setup(arg => arg.IsOverwrite)
					.As('o', "overwrite")
					.SetDefault(true);
			}
			if (processType == ProcessType.EditParticipantsByKey || processType == ProcessType.EditParticipants)
			{
				parser.Setup(arg => arg.SearchKey)
					.As('k', "searchkey")
					.Required();
			}
			if (processType == ProcessType.DeleteCasesByDate || processType == ProcessType.DeleteParticipantsByDate)
			{
				parser.Setup(arg => arg.Date)
					.As('d', "date")
					.Required();
			}
			if (processType == ProcessType.CreateParticipants)
			{
				parser.Setup(arg => arg.ParticipantType)
					.As('z', "participantType")
					.Required();
			}		

			var result = parser.Parse(args);
			if (result.HasErrors) throw new ArgumentException();

			return new Engine(parser.Object);
		}

		public Engine Run()
		{
			var state = ProcessConveyor.Create(_arguments).Run();
			if (state) Logger.Info($"{DateTime.Now} | {_arguments.ProcessType} successfully processed. Press any key to continue.");
			Console.ReadKey();
			return new Engine(_arguments);
		}		
	}
}
