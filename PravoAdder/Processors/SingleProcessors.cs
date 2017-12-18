using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Repositories;
using PravoAdder.Domain;
using PravoAdder.Helpers;
using PravoAdder.Readers;

namespace PravoAdder.Processors
{
	public class SingleProcessors
	{
		public static ParticipantProcessor Participant;
		public static ProjectProcessor Project;
		public static CoreProcessors Core;
		public static FormatProcessors Format;

		private static List<ActivityTag> _activityTags;

		static SingleProcessors()
		{
			Participant = new ParticipantProcessor();
			Project = new ProjectProcessor();
			Core = new CoreProcessors();
			Format = new FormatProcessors();
		}	

		public static Func<EngineMessage, EngineMessage> DeleteFolder = message =>
		{
			message.ApiEnviroment.DeleteProjectItem(message.Item.Id);
			return message;
		};

		public static Func<EngineMessage, EngineMessage> DeleteProjectGroup = message =>
		{
			if (!message.Item.Equals(ProjectGroup.Empty))
			{
				message.ApiEnviroment.DeleteProjectGroupItem(message.Item.Id);
			}
			return message;
		};				

		public static Func<EngineMessage, EngineMessage> CreateTask = message =>
		{
			var task = message.GetCreatable<Task>();
			ApiRouter.Task.Create(message.Authenticator, task);
			if (task.IsArchive) ApiRouter.Projects.Archive(message.Authenticator, task.Project.Id);
			return message;
		};

		public static Func<EngineMessage, EngineMessage> AddVisualBlockLine = message =>
		{
			var line = message.GetCreatable<VisualBlockLineModel>();
			if (line == null) return null;

			var sumWidth = line.Fields.Sum(f => f?.Width);
			if (sumWidth == 0) return null;

			var creator = (VisualBlockLineCreator) message.Creators[typeof(VisualBlockLineCreator).Name];
			if (sumWidth < 12)
			{
				creator.ConstructedLineModel = line;
				return message;
			}
			if (sumWidth == 12)
			{
				creator.ConstructedLineModel = null;

				if (creator.VisualBlock.Lines.Any(l => l.Fields.SequenceEqual(line.Fields)))
				{
					return null;
				}

				creator.VisualBlock.Lines.Add(line);
				var result = ApiRouter.VisualBlocks.Update(message.Authenticator, creator.VisualBlock);
				message.Item = result;				
				return message;
			}
			if (sumWidth > 12)
			{
				creator.ConstructedLineModel = null;
			}
			return null;
		};

		public static Func<EngineMessage, EngineMessage> CreateEvent = message =>
		{
			var newEvent = message.GetCreatable<Event>(message.Item);
			if (newEvent == null) return null;

			var timeLogs = ApiRouter.TimeLogs.GetMany(message.Authenticator, newEvent.Project.Id);
			var events = timeLogs.Select(x => ApiRouter.Events.Get(message.Authenticator, x.EntityId));
			if (events.Any(x => x.Name == newEvent.Name))
			{
				message.IsContinue = true;
				return message;
			}

			var isRestored = false;		
			if (newEvent.Project.IsArchive)
			{
				ApiRouter.Projects.Restore(message.Authenticator, newEvent.Project.Id);
				isRestored = true;
			}
			newEvent = ApiRouter.Events.Create(message.Authenticator, newEvent);
			if (isRestored)
			{
				ApiRouter.Projects.Archive(message.Authenticator, newEvent.Project.Id);
			}
			message.Item = newEvent.Project;
			return message;
		};

		public static Func<EngineMessage, EngineMessage> DeleteEvent = message =>
		{
			var groupItem = (GroupItem) message.Item;
			if (!groupItem.Date.Date.Equals(new DateTime(2017, 11, 10))) return null;

			ApiRouter.Events.Delete(message.Authenticator, groupItem.EntityId);
			return message;
		};

		public static Func<EngineMessage, EngineMessage> CreateTimeLog = message =>
		{
			if (_activityTags == null) _activityTags = ApiRouter.Bootstrap.GetActivityTags(message.Authenticator);

			var timeValue = message.GetValueFromRow("Timer");
			int time;
			if (timeValue == null)
			{
				var rate = double.Parse(message.GetValueFromRow("Rate"));
				var total = double.Parse(message.GetValueFromRow("Total"));
				time = (int) Math.Round(rate / total * 60);
			}
			else
			{
				if (!int.TryParse(timeValue, out time)) return null;			
			}

			var newTimeLog = new TimeLog
			{
				LogDate = DateTime.Parse(message.GetValueFromRow("Log Date")),
				Tag = _activityTags.First(t => t.Name.Equals("Event")),
				Time = time
			};

			try
			{
				newTimeLog.LogType = EventTypeRepository.GetOrPut(message.Authenticator, message.GetValueFromRow("Activity Type"));
			}
			catch (Exception)
			{
				// ignored
			}

			message.Item = ApiRouter.TimeLogs.Create(message.Authenticator, newTimeLog);
			return message;
		};

		public static Func<EngineMessage, EngineMessage> CreateDictionary = message =>
		{
			var dictionaryName = message.GetValueFromRow("Name");
			if (string.IsNullOrEmpty(dictionaryName)) return null;

			var dictionary = DictionaryRepository.GetOrCreate(message.Authenticator, dictionaryName, new DictionaryInfo { Name = dictionaryName });

			var dictionaryItemName = message.GetValueFromRow("Value");
			var dictItems = ApiRouter.DictionaryItems.GetMany(message.Authenticator, dictionary.SystemName);

			if (dictionary.Items == null) dictionary.Items = new List<DictionaryItem>(dictItems);

			if (!dictItems.Any(d => d.Name.Equals(dictionaryItemName)))
			{
				var newItem = ApiRouter.DictionaryItems.Create(message.Authenticator,
					new DictionaryItem {SystemName = dictionary.SystemName, Name = dictionaryItemName});
				dictionary.Items.Add(newItem);
			}			

			message.Item = dictionary;
			return message;
		};

		public static Func<EngineMessage, EngineMessage> CreateExcelRow = message =>
		{
			const string pageName = "Content";
			var newTableInfo = new FileInfo("output.xlsx");				
			var excelPackage = new ExcelPackage(newTableInfo);
			var headProperties = typeof(HeaderBlockInfo).GetProperties();

			ExcelWorksheet worksheet = null;
			if (!newTableInfo.Exists)
			{
				if (!HeaderBlockInfo.Languages.ContainsKey(message.Settings.Language))
				{
					message.IsFinal = true;
					return message;
				}
				var lKey = HeaderBlockInfo.Languages[message.Settings.Language];
				worksheet = excelPackage.Workbook.Worksheets.Add(pageName);
				var alphabet = Enumerable.Range(0, 26).Select(i => Convert.ToChar('A' + i));			

				foreach (var p in headProperties.Zip(alphabet, (info, c) => new {Info = info.GetAttribute<FieldNameAttribute>(), Letter = c}))
				{
					worksheet.Cells[$"{p.Letter}1"].Value = $"-b {HeaderBlockInfo.SystemNames[lKey]} -f {p.Info.FieldNames[lKey]}";

					worksheet.Cells[$"{p.Letter}1"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
					worksheet.Cells[$"{p.Letter}1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
					worksheet.Cells[$"{p.Letter}1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
					worksheet.Cells[$"{p.Letter}1"].Style.WrapText = true;
					worksheet.Cells[$"{p.Letter}1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
					worksheet.Cells[$"{p.Letter}1"].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
				}
			}
			if (worksheet == null) worksheet = excelPackage.Workbook.Worksheets.First(w => w.Name == pageName);

			var project = ApiRouter.Projects.Get(message.Authenticator, message.Item.Id);
			var projectProperties = typeof(Project).GetProperties();

			foreach (var p in headProperties.Zip(Enumerable.Range(1, headProperties.Length + 1), (info, i) => new {Info = info, Count = i}))
			{
				var projectProperty = projectProperties.FirstOrDefault(prop => prop.Name == p.Info.Name);
				if (projectProperty == null) continue;

				var value = projectProperty.PropertyType.BaseType == typeof(DatabaseEntityItem)
					? ((DatabaseEntityItem) projectProperty.GetValue(project))?.Name
					: Convert.ChangeType(projectProperty.GetValue(project), projectProperty.PropertyType)?.ToString();
				worksheet.Cells[message.Count + 2, p.Count].Value = value;
			}

			excelPackage.Save();
			message.Item = project;
			return message;
		};		

		public static Func<EngineMessage, EngineMessage> CreateExpense = message =>
		{
			Project project;
			if (message.Item != null && message.Item.GetType() == typeof(Project))
			{
				project = (Project) message.Item;
			}
			else
			{
				var projectName = Table.GetValue(message.Table.Header, message.Row, "Case Name");
				project = ProjectRepository.Get(message.Authenticator, projectName);
				if (project == null) return null;
			}		

			var name = message.GetValueFromRow("Expense");
			if (string.IsNullOrEmpty(name)) return null;

			var expense = new Expense
			{
				Amount = double.Parse(message.GetValueFromRow("Amount")),
				Date = DateTime.Parse(message.GetValueFromRow("Date")),
				Name = name,
				Project = project,
				Files = new List<string>(),
				Description = message.GetValueFromRow("Description")
			};

			message.Item = ApiRouter.Expenses.Create(message.Authenticator, expense);

			return message;
		};

		public static Func<EngineMessage, EngineMessage> UpdateBillRate = message =>
		{
			var rateValue = message.GetValueFromRow("Rate");
			if (rateValue == null || message.Item == null || message.Item.GetType() != typeof(Bill)) return null;

			var billedTime = ApiRouter.BilledTimes.GetMany(message.Authenticator, message.Item.Id).FirstOrDefault();
			if (billedTime == null) return null;

			var bill = ApiRouter.Bills.Get(message.Authenticator, message.Item.Id);
			var billStatus = bill.BillStatus;

			var statusChanged = false;
			if (billStatus.Name == "Paid")
			{
				ApiRouter.Bills.UpdateStatus(message.Authenticator,
					new BillStatusGroup {BillIds = new List<string> {bill.Id}, BillStatusSysName = "DRAFT"});
				statusChanged = true;
			}

			billedTime.Rate = double.Parse(rateValue);				

			ApiRouter.BilledTimes.Update(message.Authenticator, billedTime);
			ApiRouter.Bills.Rebuild(message.Authenticator, message.Item.Id);

			if (statusChanged)
			{
				ApiRouter.Bills.UpdateStatus(message.Authenticator,
					new BillStatusGroup { BillIds = new List<string> { bill.Id }, BillStatusSysName = billStatus.SysName });
			}

			message.Item = bill.Project;
			return message;
		};

		public static Func<EngineMessage, EngineMessage> CreateBill = message =>
		{
			if (message.Item != null && message.Item.GetType() != typeof(Project)) throw new ArgumentException();

			try
			{
				var invoicedValue = message.GetValueFromRow("Invoiced");
				var status = bool.TryParse(invoicedValue, out var isInvoice);
				if (status && isInvoice) message.Item = message.GetCreatable<Bill>(message.Item);
			}
			catch (Exception)
			{
				message.Item = message.GetCreatable<Bill>(message.Item);
			}
			
			return message;
		};

		public static Func<EngineMessage, EngineMessage> UpdateBillingSettings = message =>
		{
			var billingSettings = ApiRouter.BillingSettings.Get(message.Authenticator);
			var newBillingRules = message.GetCreatable<BillingRuleWrapper>().BillingRules;
			billingSettings.BillingRules.AddRange(newBillingRules);

			message.Item = ApiRouter.BillingSettings.Put(message.Authenticator, billingSettings);
			return message;
		};
	}
}