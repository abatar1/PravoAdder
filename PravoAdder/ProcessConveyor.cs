using System;
using System.Collections.Generic;
using System.Linq;
using PravoAdder.Domain;
using PravoAdder.Processors;

namespace PravoAdder
{
	public class ProcessConveyor
	{
		private List<ConveyorItem> Conveyor { get; }
		private EngineMessage FirstMessage { get; }

		public ProcessConveyor(ApplicationArguments arguments)
		{
			FirstMessage = new EngineMessage { Args = arguments };
			Conveyor = new List<ConveyorItem>();
		}

		public void Add(Func<EngineMessage, EngineMessage> processor, int depth = 0)
		{
			Conveyor.Add(new ConveyorItem {Depth = depth, Processor = processor, Message = new EngineMessage()});
		}

		public void AddRange(List<Func<EngineMessage, EngineMessage>> processors)
		{
			foreach (var processor in processors)
			{
				Add(processor);
			}
		}

		public void Run()
		{
			var conveyorСounter = 0;
			using (var messageConveyor = FirstMessage)
			{
				foreach (var conveyorIter in Conveyor)
				{
					if (conveyorСounter == 0 && conveyorIter.Depth != 0)
						throw new ConveyorException("Conveyor can't starts with process which depth more than 0.");

					if (conveyorIter.Depth == 0)
					{
						messageConveyor.Concat(conveyorIter.Message);
						var responseMessage = conveyorIter.Processor(messageConveyor);
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
		}

		public static ProcessConveyor Create(ApplicationArguments arguments)
		{
			var conveyor = new ProcessConveyor(arguments);
			var processType = arguments.ProcessType;

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
					conveyor.AddRange(GroupedProcessors.LoadWithTable);
					conveyor.Add(SingleProcessors.Project.Update, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Row);
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
					conveyor.AddRange(GroupedProcessors.LoadWithTable);
					conveyor.Add(SingleProcessors.CreateTask, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Row);
					break;					
				case ProcessType.CreateParticipants:					
					conveyor.AddRange(GroupedProcessors.LoadWithTable);
					conveyor.Add(SingleProcessors.Participant.Create, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Row);
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
					conveyor.AddRange(GroupedProcessors.LoadWithTable);
					conveyor.Add(SingleProcessors.Project.AddNote, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Row);
					break;
				case ProcessType.EditParticipantsByKey:
					conveyor.AddRange(GroupedProcessors.LoadWithTable);
					conveyor.Add(SingleProcessors.Participant.EditById, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Row);
					break;
				case ProcessType.RenameCases:
					conveyor.AddRange(GroupedProcessors.LoadWithTable);
					conveyor.Add(SingleProcessors.Project.Rename, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Row);
					break;
				case ProcessType.AttachParticipant:
					conveyor.AddRange(GroupedProcessors.LoadWithTable);
					conveyor.Add(SingleProcessors.Project.AttachParticipant, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Row);
					break;
				case ProcessType.EditParticipants:
					conveyor.AddRange(GroupedProcessors.LoadWithTable);
					conveyor.Add(SingleProcessors.Participant.Edit, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Row);
					break;
			}
			return conveyor;
		}
	}
}
