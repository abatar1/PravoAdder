﻿using System;
using System.Collections.Generic;
using System.Linq;
using PravoAdder.Domain;

namespace PravoAdder.Processors
{
	public class ProcessConveyor
	{
		private List<ConveyorItem> Conveyor { get; }
		private EngineMessage FirstMessage { get; }

		public ProcessConveyor(ApplicationArguments arguments)
		{
			FirstMessage = new EngineMessage { ApplicationArguments = arguments };
			Conveyor = new List<ConveyorItem>();
		}

		public void Add(Func<EngineMessage, EngineMessage> processor, int depth = 0)
		{
			Conveyor.Add(new ConveyorItem { Depth = depth, Processor = processor, Message = new EngineMessage()});		
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
					if (conveyorСounter == 0 || conveyorIter.Depth == 0)
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
					conveyor.Add(SingleProcessors.TryCreateProject, 1);
					conveyor.Add(SingleProcessors.AddInformation, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Row);
					break;					
				case ProcessType.Update:					
					conveyor.AddRange(GroupedProcessors.LoadWithTable);
					conveyor.Add(SingleProcessors.UpdateProject, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Row);
					break;				
				case ProcessType.Sync:					
					conveyor.AddRange(GroupedProcessors.LoadWithTable);
					conveyor.Add(SingleProcessors.TryCreateProject, 1);
					conveyor.Add(SingleProcessors.SynchronizeProject, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Row);
					break;					
				case ProcessType.CleanAll:					
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
				case ProcessType.CleanByDate:					
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.DeleteProject, 2);
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
				case ProcessType.CreateParticipant:					
					conveyor.AddRange(GroupedProcessors.LoadWithTable);
					conveyor.Add(SingleProcessors.CreateParticipant, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Row);
					break;					
				case ProcessType.DeleteAllParticipant:
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.DeleteParticipant, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Participant);
					break;		
				case ProcessType.DeleteParticipantByDate:
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.DeleteParticipant, 1);
					conveyor.Add(SingleProcessors.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.ParticipantByDate);
					break;
				case ProcessType.DistinctParticipant:					
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.DistinctParticipants);
					break;					
			}
			return conveyor;
		}
	}
}
