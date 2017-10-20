using System;
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

		public Engine Initialize(string[] args)
		{
			if (args.Length == 0) throw new ArgumentException();

			var parser = new FluentCommandLineParser<ApplicationArguments>();
		
			parser.Setup(arg => arg.ProcessType)
				.As('t', "type")
				.Required();
			parser.Parse(args);
			var processType = parser.Object.ProcessType;
			if (processType == ProcessType.Migration || processType == ProcessType.Sync || processType == ProcessType.CreateParticipant)
			{
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
			if (processType == ProcessType.CleanByDate || processType == ProcessType.DeleteParticipantByDate)
			{
				parser.Setup(arg => arg.Date)
					.As('d', "date")
					.Required();
			}
			if (processType == ProcessType.CreateParticipant)
			{
				parser.Setup(arg => arg.ParticipantType)
					.As('z', "participantType")
					.Required();
			}
			parser.Setup(arg => arg.ParallelOptions)
				.As('p', "parallel")
				.SetDefault(1);
			parser.Setup(arg => arg.ConfigFilename)
				.As('c', "config")
				.SetDefault("config.json");

			var result = parser.Parse(args);
			if (result.HasErrors) throw new ArgumentException();

			return new Engine(parser.Object);
		}

		public Engine Run()
		{
			ProcessConveyor.Create(_arguments).Run();
			Logger.Info($"{DateTime.Now} | {_arguments.ProcessType} successfully processed. Press any key to continue.");
			Console.ReadKey();
			return new Engine(_arguments);
		}		
	}
}
