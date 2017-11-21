using System;
using Fclp;
using NLog;
using PravoAdder.Domain;

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

			parser.Setup(arg => arg.UserName)
				.As('b', "baseuri")
				.Required();
			parser.Setup(arg => arg.UserName)
				.As('u', "username")
				.Required();
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

			var processType = ProcessTypes.GetByName(parser.Object.ProcessType);
			if (processType.NeedTable)
			{
				parser.Setup(arg => arg.SourceFileName)
					.As('s', "sourcefile")
					.Required();
				parser.Setup(arg => arg.ReaderMode)
					.As('m', "mode")
					.Required();
				parser.Setup(arg => arg.RowNum)
					.As('r', "row")
					.SetDefault(0);
				parser.Setup(arg => arg.IsOverwrite)
					.As('o', "overwrite")
					.SetDefault(true);
			}
			if (processType.Name == "ParticipantEditByKey" || processType.Name == "ParticipantEdit")
			{
				parser.Setup(arg => arg.SearchKey)
					.As('k', "searchkey")
					.Required();
			}
			if (processType.Name == "CaseDeleteByDate" || processType.Name == "ParticipantDeleteByDate")
			{
				parser.Setup(arg => arg.Date)
					.As('k', "date")
					.Required();
			}

			if (processType.Name == "CaseUnload")
			{
				parser.Setup(arg => arg.ProjectType)
					.As('k', "projectType")
					.Required();
				parser.Setup(arg => arg.Language)
					.As('l', "language")
					.Required();
			}

			if (processType.Name == "ParticipantCreate")
			{
				parser.Setup(arg => arg.ParticipantType)
					.As('k', "participantType")
					.Required();
			}		

			var result = parser.Parse(args);
			if (result.HasErrors) throw new ArgumentException();

			return new Engine(parser.Object);
		}

		public Engine Run()
		{
			var conveyor = new ProcessConveyor(_arguments);
			var isSuccess = conveyor.Create().Run();
			if (isSuccess) Logger.Info($"{DateTime.Now} | {_arguments.ProcessType} successfully processed. Press any key to continue.");

			Console.ReadKey();
			return new Engine(_arguments);
		}		
	}
}
