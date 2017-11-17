﻿using System;
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

		public void AddRange(IEnumerable<Func<EngineMessage, EngineMessage>> processors, int depth = 0)
		{
			foreach (var processor in processors)
			{
				Add(processor, depth);
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

		private void SetTableProcessor(params Func<EngineMessage, EngineMessage>[] processors)
		{
			AddRange(GroupedProcessors.LoadWithTable);
			AddRange(processors, 1);
			Add(SingleProcessors.Core.ProcessCount, 1);
			Add(ForEachProcessors.Row);
		}

		public ProcessConveyor Create()
		{
			var conveyor = new ProcessConveyor(_args);
			var processType = _args.ProcessType;

			Console.Title = $@"Pravo.{Enum.GetName(typeof(ProcessType), processType)}";

			switch (processType)
			{
				case ProcessType.CaseCreate:
					SetTableProcessor(SingleProcessors.Project.TryCreate, SingleProcessors.Project.AddInformation);
					break;					
				case ProcessType.CaseUpdate:
					SetTableProcessor(SingleProcessors.Project.Update);
					break;				
				case ProcessType.CaseSync:
					SetTableProcessor(SingleProcessors.Project.TryCreate, SingleProcessors.Project.Synchronize);
					break;					
				case ProcessType.CaseDelete:					
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.Project.Delete, 2);
					conveyor.Add(SingleProcessors.Core.ProcessCount, 2);
					conveyor.Add(ForEachProcessors.Project, 1);
					conveyor.Add(SingleProcessors.DeleteProjectGroup, 1);
					conveyor.Add(ForEachProcessors.ProjectGroup);
					conveyor.Add(SingleProcessors.DeleteFolder, 1);
					conveyor.Add(SingleProcessors.Core.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Folder);
					break;					
				case ProcessType.CaseDeleteByDate:					
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.Project.Delete, 2);
					conveyor.Add(SingleProcessors.Core.ProcessCount, 2);
					conveyor.Add(ForEachProcessors.ProjectByDate, 1);
					conveyor.Add(ForEachProcessors.ProjectGroup);
					break;					
				case ProcessType.TaskCreate:
					SetTableProcessor(SingleProcessors.CreateTask);
					break;					
				case ProcessType.ParticipantCreate:
					SetTableProcessor(SingleProcessors.Participant.Create);
					break;					
				case ProcessType.ParticipantDelete:
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.Participant.Delete, 1);
					conveyor.Add(SingleProcessors.Core.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Participant);
					break;		
				case ProcessType.ParticipantDeleteByDate:
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.Participant.Delete, 1);
					conveyor.Add(SingleProcessors.Core.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.ParticipantByDate);
					break;
				case ProcessType.ParticipantDistinct:					
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.Participant.Distinct);
					break;			
				case ProcessType.HeaderAnalyze:
					conveyor.AddRange(GroupedProcessors.LoadWithTable);
					conveyor.Add(SingleProcessors.AnalyzeHeader);
					break;
				case ProcessType.NoteCreate:
					SetTableProcessor(SingleProcessors.Project.AddNote);
					break;
				case ProcessType.ParticipantEditByKey:
					SetTableProcessor(SingleProcessors.Participant.EditById);
					break;
				case ProcessType.CaseRename:
					SetTableProcessor(SingleProcessors.Project.Rename);
					break;
				case ProcessType.ParticipantAttach:
					SetTableProcessor(SingleProcessors.Project.AttachParticipant);
					break;
				case ProcessType.ParticipantEdit:
					SetTableProcessor(SingleProcessors.Participant.Edit);
					break;
				case ProcessType.ProjectFieldCreate:
					SetTableProcessor(ProjectProcessor.CreateProjectField);
					break;
				case ProcessType.VisualBlockLineAdd:
					SetTableProcessor(SingleProcessors.AddVisualBlockLine);
					break;
				case ProcessType.CaseUnload:
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.CreateExcelRow, 2);
					conveyor.Add(SingleProcessors.Core.ProcessCount, 2);
					conveyor.Add(ForEachProcessors.ProjectByType, 1);
					conveyor.Add(ForEachProcessors.ProjectGroup);
					break;
				case ProcessType.DictionaryCreate:
					SetTableProcessor(SingleProcessors.CreateDictionary);
					break;
				case ProcessType.CaseTypeCreate:
					SetTableProcessor(SingleProcessors.Project.CreateType);
					break;
				case ProcessType.EventCreate:
					SetTableProcessor(SingleProcessors.CreateTimeLog, SingleProcessors.CreateEvent);
					break;
				case ProcessType.EventDelete:
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.DeleteEvent, 1);
					conveyor.Add(SingleProcessors.Core.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Event);
					break;
				case ProcessType.CaseUpdateSettings:
					SetTableProcessor(SingleProcessors.Project.UpdateSettings);
					break;
				case ProcessType.ExpenseCreate:
					SetTableProcessor(SingleProcessors.CreateExpense);
					break;
				case ProcessType.BillingRuleUpdate:
					SetTableProcessor(SingleProcessors.UpdateBillingSettings);
					break;
				case ProcessType.BillCreate:
					SetTableProcessor(SingleProcessors.CreateBill);
					break;
				default:
					throw new ArgumentException("Неизвестный тип конвеера.");
			}
			return conveyor;
		}
	}
}
