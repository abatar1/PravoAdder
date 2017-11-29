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
			var task = (Task) message.GetCreatable();
			ApiRouter.Task.Create(message.Authenticator, task);
			if (task.IsArchive) ApiRouter.Projects.Archive(message.Authenticator, task.Project.Id);
			return message;
		};

		public static Func<EngineMessage, EngineMessage> AddVisualBlockLine = message =>
		{
			var line = (VisualBlockLine) message.GetCreatable();
			if (line == null) return null;

			var sumWidth = line.Fields.Sum(f => f?.Width);
			if (sumWidth == 0) return null;

			var creator = (VisualBlockLineCreator) message.Creator;
			if (sumWidth < 12)
			{
				creator.ConstructedLine = line;
				return message;
			}
			if (sumWidth == 12)
			{
				creator.ConstructedLine = null;

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
				creator.ConstructedLine = null;
			}
			return null;
		};

		public static Func<EngineMessage, EngineMessage> CreateEvent = message =>
		{
			var newEvent = (Event) message.GetCreatable(message.Item);
			if (newEvent == null) return null;

			var isRestored = false;			
			if (newEvent.Project.IsArchive)
			{
				ApiRouter.Projects.Restore(message.Authenticator, newEvent.Project.Id);
				isRestored = true;
			}
			message.Item = ApiRouter.Events.Create(message.Authenticator, newEvent);
			if (isRestored)
			{
				ApiRouter.Projects.Archive(message.Authenticator, newEvent.Project.Id);
			}
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
			var activityType = message.GetValueFromRow("Activity Type");
			if (string.IsNullOrEmpty(activityType)) return null;

			var logType = EventTypeRepository.GetOrPut(message.Authenticator, message.GetValueFromRow("Activity Type"));
			if (logType == null) return null;

			var newTimeLog = new TimeLog
			{
				LogType = logType,
				LogDate = message.GetValueFromRow("Log Date").FormatDate(),
				Tag = _activityTags.First(t => t.Name.Equals("Event")),
				Time = int.Parse(message.GetValueFromRow("Timer"))
			};

			message.Item = ApiRouter.TimeLogs.Create(message.Authenticator, newTimeLog);
			return message;
		};

		public static Func<EngineMessage, EngineMessage> CreateDictionary = message =>
		{
			var dictionaryName = message.GetValueFromRow("Name");
			if (string.IsNullOrEmpty(dictionaryName)) return null;

			var dictionary = DictionaryRepository.GetOrCreate<DictionaryApi>(message.Authenticator, dictionaryName, new DictionaryInfo { Name = dictionaryName });

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
				if (!HeaderBlockInfo.Languages.ContainsKey(message.Args.Language))
				{
					message.IsFinal = true;
					return message;
				}
				var lKey = HeaderBlockInfo.Languages[message.Args.Language];
				worksheet = excelPackage.Workbook.Worksheets.Add(pageName);
				var alphabet = Enumerable.Range(0, 26).Select(i => Convert.ToChar('A' + i));			

				foreach (var p in headProperties.Zip(alphabet, (info, c) => new {Info = info.LoadAttribute<FieldNameAttribute>(), Letter = c}))
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
			var projectName = Table.GetValue(message.Table.Header, message.Row, "Case Name");
			var project = ProjectRepository.Get<ProjectsApi>(message.Authenticator, projectName);
			if (project == null) return null;

			var name = message.GetValueFromRow("Expense");
			if (string.IsNullOrEmpty(name)) return null;

			var expense = new Expense
			{
				Amount = double.Parse(message.GetValueFromRow("Amount")),
				Date = DateTime.Parse(message.GetValueFromRow("Date")),
				Name = name,
				Project = project,
				Files = new List<string>()
			};

			message.Item = ApiRouter.Expenses.Create(message.Authenticator, expense);
			return message;
		};

		public static Func<EngineMessage, EngineMessage> CreateBill = message =>
		{
			var bill = (Bill) message.Creator.Create(message.Table.Header, message.Row, message.Item);
			message.Item = bill;
			return message;
		};

		public static Func<EngineMessage, EngineMessage> UpdateBillingSettings = message =>
		{
			var billingSettings = ApiRouter.BillingSettings.Get(message.Authenticator);
			var newBillingRules = ((BillingRuleWrapper) message.GetCreatable()).BillingRules;
			billingSettings.BillingRules.AddRange(newBillingRules);

			message.Item = ApiRouter.BillingSettings.Put(message.Authenticator, billingSettings);
			return message;
		};
	}
}