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
		private static Settings _settings;

		public ProcessConveyor(Settings settings)
		{
			FirstMessage = new EngineMessage { Settings = settings };
			_settings = settings;
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
							                  .FindIndex(iter => conveyorIter.Depth - iter.Depth == 1);
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
			var conveyor = new ProcessConveyor(_settings);
			var processType = _settings.ProcessType;

			Console.Title = $@"Pravo.{ProcessTypes.GetByName(processType).Name}";

			switch (processType)
			{
				case "CaseCreate":
					SetTableProcessor(SingleProcessors.Project.TryCreate, SingleProcessors.Project.AddInformation);
					break;					
				case "CaseUpdate":
					SetTableProcessor(SingleProcessors.Project.Update, SingleProcessors.Project.UpdateInformation);
					break;				
				case "CaseSync":
					SetTableProcessor(SingleProcessors.Project.TryCreate, SingleProcessors.Project.Synchronize);
					break;					
				case "CaseDelete":					
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
				case "CaseDeleteByDate":					
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.Project.Delete, 1);
					conveyor.Add(SingleProcessors.Core.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.ProjectByDate);
					break;					
				case "TaskCreate":
					SetTableProcessor(SingleProcessors.CreateTask);
					break;					
				case "ParticipantCreate":
					SetTableProcessor(SingleProcessors.Participant.Create);
					break;					
				case "ParticipantDelete":
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.Participant.Delete, 1);
					conveyor.Add(SingleProcessors.Core.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Participant);
					break;		
				case "ParticipantDeleteByDate":
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.Participant.Delete, 1);
					conveyor.Add(SingleProcessors.Core.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.ParticipantByDate);
					break;
				case "ParticipantDistinct":					
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.Participant.Distinct);
					break;			
				case "HeaderAnalyze":
					conveyor.AddRange(GroupedProcessors.LoadWithTable);
					conveyor.Add(SingleProcessors.Format.AnalyzeHeader);
					break;
				case "NoteCreate":
					SetTableProcessor(SingleProcessors.Project.AddNote);
					break;
				case "ParticipantEditByKey":
					SetTableProcessor(SingleProcessors.Participant.EditById);
					break;
				case "CaseRename":
					SetTableProcessor(SingleProcessors.Project.Rename);
					break;
				case "ParticipantAttach":
					SetTableProcessor(SingleProcessors.Project.AttachParticipant);
					break;
				case "ParticipantEdit":
					SetTableProcessor(SingleProcessors.Participant.Edit);
					break;
				case "ProjectFieldCreate":
					SetTableProcessor(SingleProcessors.Project.CreateProjectField);
					break;
				case "VisualBlockLineAdd":
					SetTableProcessor(SingleProcessors.AddVisualBlockLine);
					break;
				case "CaseUnload":
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.CreateExcelRow, 2);
					conveyor.Add(SingleProcessors.Core.ProcessCount, 2);
					conveyor.Add(ForEachProcessors.ProjectByType, 1);
					conveyor.Add(ForEachProcessors.ProjectGroup);
					break;
				case "DictionaryCreate":
					SetTableProcessor(SingleProcessors.CreateDictionary);
					break;
				case "CaseTypeCreate":
					SetTableProcessor(SingleProcessors.Project.CreateType);
					break;
				case "EventCreate":
					SetTableProcessor(SingleProcessors.CreateTimeLog, SingleProcessors.CreateEvent);
					break;
				case "EventDelete":
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.DeleteEvent, 1);
					conveyor.Add(SingleProcessors.Core.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Event);
					break;
				case "CaseUpdateSettings":
					SetTableProcessor(SingleProcessors.Project.UpdateSettings);
					break;
				case "ExpenseCreate":
					SetTableProcessor(SingleProcessors.CreateExpense);
					break;
				case "BillingRuleUpdate":
					SetTableProcessor(SingleProcessors.UpdateBillingSettings);
					break;
				case "BillCreate":
					SetTableProcessor(SingleProcessors.Project.SetClient, SingleProcessors.CreateBill);
					break;
				case "ExpenseCreateMany":
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.Format.Expenses, 1);
					conveyor.Add(SingleProcessors.Core.LoadTable, 1);					
					conveyor.Add(SingleProcessors.Project.SetClient, 2);
					conveyor.Add(SingleProcessors.CreateExpense, 2);		
					conveyor.Add(SingleProcessors.Core.ProcessCount, 2);
					conveyor.Add(ForEachProcessors.Row, 1);				
					conveyor.Add(ForEachProcessors.File);
					break;
				case "DocumentUpload":
					conveyor.AddRange(GroupedProcessors.LoadWithFormattedTable(SingleProcessors.Format.Case));
					conveyor.Add(SingleProcessors.Project.Files, 1);
					conveyor.Add(SingleProcessors.Core.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Row);
					break;
				case "DocumentToMultiline":
					conveyor.AddRange(GroupedProcessors.LoadWithoutTable);
					conveyor.Add(SingleProcessors.Project.DocumentsToMultilines, 1);
					conveyor.Add(SingleProcessors.Core.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Project);
					break;
				case "BillingEventCreate":
					SetTableProcessor(SingleProcessors.CreateTimeLog, SingleProcessors.CreateEvent, 
						SingleProcessors.CreateBill, SingleProcessors.UpdateBillRate);
					break;
				case "CreateBillsByClientName":
					conveyor.AddRange(GroupedProcessors.LoadWithTable);
					conveyor.Add(SingleProcessors.Project.GetProjectByClientName, 1);
					conveyor.Add(SingleProcessors.CreateExpense, 1);
					conveyor.Add(SingleProcessors.CreateBill, 1);
					conveyor.Add(SingleProcessors.Core.ProcessCount, 1);
					conveyor.Add(ForEachProcessors.Row);
					break;
				case "PracticeAreaCheck":
					conveyor.AddRange(GroupedProcessors.LoadWithTable);
					conveyor.Add(SingleProcessors.Format.CheckPracticeAreas);
					break;
				default:
					throw new ArgumentException("Неизвестный тип конвеера.");
			}
			return conveyor;
		}
	}
}
