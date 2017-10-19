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
<<<<<<< HEAD
			if (processType == ProcessType.Migration || processType == ProcessType.Sync || processType == ProcessType.CreateParticipant)
=======
			if (processType == ProcessType.Migration || processType == ProcessType.Sync ||
			    processType == ProcessType.Participant || processType == ProcessType.Task)
>>>>>>> e06ccc4eb4c20f5b0a884c8c73b5e112fbac295a
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
			if (parser.Object.ProcessType == ProcessType.CreateParticipant)
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
			CreateConveyor(_arguments).Run();
			Logger.Info($"{DateTime.Now} | {_arguments.ProcessType} successfully processed. Press any key to continue.");
			Console.ReadKey();
			return new Engine(_arguments);
		}	

		private static ProcessConveyor CreateConveyor(ApplicationArguments arguments)
		{
			var conveyor = new ProcessConveyor(arguments);
			var processType = arguments.ProcessType;

			Console.Title = $"Pravo.{Enum.GetName(typeof(ProcessType), processType)}";

			switch (processType)
			{					
				case ProcessType.Migration:
				{
					conveyor.AddRange(GroupedProcessors.LoadWithTable);
					conveyor.Add(SingleProcessors.TryCreateProject, 1);
					conveyor.Add(SingleProcessors.AddInformation, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Row);
					break;
				}
				case ProcessType.Update:
				{
					conveyor.AddRange(GroupedProcessors.LoadWithTable);
					conveyor.Add(SingleProcessors.UpdateProject, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Row);
					break;
				}
				case ProcessType.Sync:
				{
					conveyor.AddRange(GroupedProcessors.LoadWithTable);
					conveyor.Add(SingleProcessors.TryCreateProject, 1);
					conveyor.Add(SingleProcessors.SynchronizeProject, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Row);
					break;
				}
				case ProcessType.CleanAll:
				{
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.DeleteProject, 2);
					conveyor.Add(SingleProcessors.ProcessCount, 2);
					conveyor.Add(ForEachProcessors.Project, 1);
					conveyor.Add(SingleProcessors.DeleteProjectGroup, 1);
					conveyor.Add(ForEachProcessors.ProjectGroup);
					conveyor.Add(SingleProcessors.DeleteFolder, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Folder);
					break;
				}
				case ProcessType.CleanByDate:
				{
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.DeleteProject, 2);
					conveyor.Add(SingleProcessors.ProcessCount, 2);
					conveyor.Add(ForEachProcessors.Project, 1);
					conveyor.Add(ForEachProcessors.ProjectGroup);
					break;
				}
				case ProcessType.CreateTask:
				{
					conveyor.AddRange(GroupedProcessors.LoadWithTable);
					conveyor.Add(SingleProcessors.CreateTask, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Row);
					break;
				}
				case ProcessType.CreateParticipant:
				{
					conveyor.AddRange(GroupedProcessors.LoadWithTable);
					conveyor.Add(SingleProcessors.CreateParticipant, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Row);
					break;
				}
<<<<<<< HEAD
				case ProcessType.DeleteAllParticipant:
				{
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.DeleteParticipant, 1);
					conveyor.Add(ForEachProcessors.Participant);
					break;
=======
				case ProcessType.Participant:
				{
					Console.Title = "Pravo.Participant";
					return new ParticipantProcessor(arguments, request =>
					{
						if (request.Participant == null) return new EngineResponse();
						request.ApiEnviroment.PutExtendentParticipant(request.Participant);		
						return new EngineResponse();
					});
>>>>>>> e06ccc4eb4c20f5b0a884c8c73b5e112fbac295a
				}
				case ProcessType.DistinctParticipant:
				{
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.DistinctParticipants);
					break;
				}
			}
			return conveyor;
		}		
	}
}
