using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using PravoAdder.Domain;
using PravoAdder.Processors;

namespace PravoAdder
{
	public class ProcessConveyor
	{
		private static List<ConveyorItem> Conveyor { get; set; }
		private EngineMessage FirstMessage { get; }
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
		private static ApplicationArguments _args;

		public ProcessConveyor(ApplicationArguments arguments)
		{
			FirstMessage = new EngineMessage { Args = arguments };
			_args = arguments;
		}

		public void Add(Func<EngineMessage, EngineMessage> processor, int depth = 0)
		{
			if (Conveyor == null) Conveyor = new List<ConveyorItem>();
			Conveyor.Add(new ConveyorItem {Depth = depth, Processor = processor, Message = new EngineMessage()});
		}

		public void AddRange(List<Func<EngineMessage, EngineMessage>> processors)
		{
			foreach (var processor in processors)
			{
				Add(processor);
			}
		}

		public bool Run()
		{
			var conveyorСounter = 0;
			using (var messageConveyor = FirstMessage)
			{
				foreach (var conveyorIter in Conveyor)
				{
					if (conveyorСounter == 0 && conveyorIter.Depth != 0)
					{
						Logger.Info("Conveyor can't starts with process which depth more than 0.");
						return false;
					}

					if (conveyorIter.Depth == 0)
					{
						messageConveyor.Concat(conveyorIter.Message);
						var responseMessage = conveyorIter.Processor(messageConveyor);
						if (responseMessage.IsFinal) return false;
						messageConveyor.Concat(responseMessage);
					}
					else
					{
						var startIndex = conveyorСounter + 1;
						var parentIndex = startIndex + Conveyor
							                  .Skip(startIndex)
							                  .ToList()
							                  .FindIndex(iter => iter.Depth != conveyorIter.Depth);
						if (Conveyor[parentIndex].Message.Child == null)
						{
							Conveyor[parentIndex].Message.Child = new List<ConveyorItem>();
						}
						conveyorIter.Message.Concat(messageConveyor);
						Conveyor[parentIndex].Message.Child.Add(conveyorIter);
					}
					conveyorСounter += 1;
				}
			}
			return true;
		}

		private void SetSimpleTableProcessor(Func<EngineMessage, EngineMessage> simpleProcessor)
		{
			AddRange(GroupedProcessors.LoadWithTable);
			Add(simpleProcessor, 1);
			Add(SingleProcessors.ProcessCount, 1);
			Add(ForEachProcessors.Row);
		}

		public ProcessConveyor Create()
		{
			var conveyor = new ProcessConveyor(_args);
			var processType = _args.ProcessType;

			Console.Title = $"Pravo.{Enum.GetName(typeof(ProcessType), processType)}";

			switch (processType)
			{
				case ProcessType.Migration:					
					conveyor.AddRange(GroupedProcessors.LoadWithTable);
					conveyor.Add(SingleProcessors.Project.TryCreate, 1);
					conveyor.Add(SingleProcessors.Project.AddInformation, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Row);
					break;					
				case ProcessType.Update:
					SetSimpleTableProcessor(SingleProcessors.Project.Update);
					break;				
				case ProcessType.Sync:					
					conveyor.AddRange(GroupedProcessors.LoadWithTable);
					conveyor.Add(SingleProcessors.Project.TryCreate, 1);
					conveyor.Add(SingleProcessors.Project.Synchronize, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Row);
					break;					
				case ProcessType.DeleteCases:					
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.Project.Delete, 2);
					conveyor.Add(SingleProcessors.ProcessCount, 2);
					conveyor.Add(ForEachProcessors.Project, 1);
					conveyor.Add(SingleProcessors.DeleteProjectGroup, 1);
					conveyor.Add(ForEachProcessors.ProjectGroup);
					conveyor.Add(SingleProcessors.DeleteFolder, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Folder);
					break;					
				case ProcessType.DeleteCasesByDate:					
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.Project.Delete, 2);
					conveyor.Add(SingleProcessors.ProcessCount, 2);
					conveyor.Add(ForEachProcessors.ProjectByDate, 1);
					conveyor.Add(ForEachProcessors.ProjectGroup);
					break;					
				case ProcessType.CreateTask:
					SetSimpleTableProcessor(SingleProcessors.CreateTask);
					break;					
				case ProcessType.CreateParticipants:
					SetSimpleTableProcessor(SingleProcessors.Participant.Create);
					break;					
				case ProcessType.DeleteParticipants:
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.Participant.Delete, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Participant);
					break;		
				case ProcessType.DeleteParticipantsByDate:
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.Participant.Delete, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.ParticipantByDate);
					break;
				case ProcessType.DistinctParticipants:					
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.Participant.Distinct);
					break;			
				case ProcessType.Analyze:
					conveyor.AddRange(GroupedProcessors.LoadWithTable);
					conveyor.Add(SingleProcessors.AnalyzeHeader);
					break;
				case ProcessType.Notes:
					SetSimpleTableProcessor(SingleProcessors.Project.AddNote);
					break;
				case ProcessType.EditParticipantsByKey:
					SetSimpleTableProcessor(SingleProcessors.Participant.EditById);
					break;
				case ProcessType.RenameCases:
					SetSimpleTableProcessor(SingleProcessors.Project.Rename);
					break;
				case ProcessType.AttachParticipant:
					SetSimpleTableProcessor(SingleProcessors.Project.AttachParticipant);
					break;
				case ProcessType.EditParticipants:
					SetSimpleTableProcessor(SingleProcessors.Participant.Edit);
					break;
				case ProcessType.CreateProjectField:
					SetSimpleTableProcessor(SingleProcessors.CreateProjectField);
					break;
				case ProcessType.AddVisualBlockRow:
					SetSimpleTableProcessor(SingleProcessors.AddVisualBlockRow);
					break;
				default:
					throw new ArgumentException("Неизвестный тип конвеера.");
			}
			return conveyor;
		}
	}
}
