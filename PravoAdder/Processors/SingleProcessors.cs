using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PravoAdder.Api.Domain;
using PravoAdder.Domain;
using PravoAdder.Domain.Attributes;
using PravoAdder.Helpers;
using PravoAdder.Readers;
using PravoAdder.Wrappers;

namespace PravoAdder.Processors
{
	public class SingleProcessors
	{
		public static ParticipantProcessor Participant;
		public static ProjectProcessor Project;		

		static SingleProcessors()
		{
			Participant = new ParticipantProcessor();
			Project = new ProjectProcessor();
		}

		public static Func<EngineMessage, EngineMessage> LoadSettings = message =>
		{
			var settingsController = new SettingsWrapper();
			return new EngineMessage
			{
				Settings = settingsController.LoadSettingsFromConsole(message.Args),
				ParallelOptions = new ParallelOptions {MaxDegreeOfParallelism = message.Args.ParallelOptions}
			};
		};

		public static Func<EngineMessage, EngineMessage> LoadTable = message =>
		{
			TableEnviroment.Initialize(message.Args, message.Settings);
			return new EngineMessage {Table = TableEnviroment.Table, Settings = message.Settings};
		};

		public static Func<EngineMessage, EngineMessage> InitializeApp = message =>
		{
			var authenticatorController = new AuthentificatorWrapper(message.Settings, message.Args);
			var authenticator = authenticatorController.Authenticate();
			if (authenticator == null)
			{
				message.IsFinal = true;
				return message;
			}

			var processType = message.Args.ProcessType;
			var processName = Enum.GetName(typeof(ProcessType), processType);
			if (processName == null) return new EngineMessage {IsFinal = true};

			ICreator creator = null;
			if (processName.Contains("Participant")) creator = new ParticipantCreator(authenticator, message.Args.ParticipantType); 
			if (processName.Contains("Task")) creator = new TaskCreator(authenticator);
			if (processName.Contains("ProjectField")) creator = new ProjectFieldCreator(authenticator);
			if (processName.Contains("VisualBlockLine")) creator = new VisualBlockLineCreator(authenticator);

			return new EngineMessage
			{
				Authenticator = authenticator,
				CaseBuilder = new CaseBuilder(message.Table, message.Settings, authenticator),
				ApiEnviroment = new ApiEnviroment(authenticator),
				Counter = new Counter(),
				Creator = creator
			};
		};

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

		public static Func<EngineMessage, EngineMessage> ProcessCount = message =>
		{
			if (message.Item == null) return message;
			message.Counter.ProcessCount(message.Count, message.Total, message.Args.RowNum, message.Item, 70);
			return message;
		};

		public static Func<EngineMessage, EngineMessage> CreateTask = message =>
		{
			var task = (Api.Domain.Task) message.GetCreatable();
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
				creator.VisualBlock.Lines.Add(line);
				var result = ApiRouter.VisualBlock.Update(message.Authenticator, creator.VisualBlock);
				message.Item = result;				
				return message;
			}
			return null;
		};

		private static List<DictionaryInfo> _dictionaries;

		public static Func<EngineMessage, EngineMessage> CreateDictionary = message =>
		{
			var dictionaryName = message.GetValueFromRow("Name");
			if (string.IsNullOrEmpty(dictionaryName)) return null;

			if (_dictionaries == null) _dictionaries = ApiRouter.Dictionary.GetMany(message.Authenticator);
			var dictionary =
				_dictionaries.FirstOrDefault(d => d.DisplayName.Equals(dictionaryName, StringComparison.InvariantCultureIgnoreCase));
			if (dictionary == null)
			{
				dictionary = ApiRouter.Dictionary.Create(message.Authenticator, new DictionaryInfo {Name = dictionaryName});
				_dictionaries.Add(dictionary);
			}

			var dictionaryItemName = message.GetValueFromRow("Value");
			var dictItems = ApiRouter.Dictionary.GetItems(message.Authenticator, dictionary.SystemName);

			if (dictionary.Items == null) dictionary.Items = new List<DictionaryItem>(dictItems);

			if (!dictItems.Any(d => d.Name.Equals(dictionaryItemName)))
			{
				var newItem = ApiRouter.Dictionary.SaveItem(message.Authenticator, dictionary.SystemName, dictionaryItemName);
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

		public static Func<EngineMessage, EngineMessage> AnalyzeHeader = message =>
		{
			var types = ApiRouter.ProjectTypes.GetMany(message.Authenticator);
			var blockTypes = new Dictionary<string, List<(string, List<string>)>>();
			var errorKeys = new List<int>();
			foreach (var cell in message.Table.Header)
			{
				if (cell.Value.IsSystem)
				{
					var systemHeaderName = typeof(HeaderBlockInfo).GetProperties()
						.Select(p => p.LoadAttribute<FieldNameAttribute>().FieldNames)
						.FirstOrDefault(p => p.Contains(cell.Value.FieldName));
					if (systemHeaderName != null) continue;
				}

				var wasAchieved = false;
				foreach (var type in types)
				{
					if (!blockTypes.ContainsKey(type.Name))
					{
						var blocks = (from block in ApiRouter.ProjectTypes.GetVisualBlocks(message.Authenticator, type.Id)
							let fields = block.Lines.SelectMany(line => line.Fields.Select(field => field.ProjectField.Name)).ToList()
							select (block.Name, fields)).ToList();
						blockTypes.Add(type.Name, blocks);
					}
					var blockName = blockTypes[type.Name]
						.FirstOrDefault(block => block.Item1 == cell.Value.BlockName && block.Item2.Contains(cell.Value.FieldName)).Item1;
					if (blockName != null)
					{
						wasAchieved = true;
						break;
					}			
				}
				if (!wasAchieved)
				{
					errorKeys.Add(cell.Key);			
				}
			}
			using (var xlPackage = new ExcelPackage(new FileInfo(message.Args.SourceFileName + ".xlsx")))
			{
				var worksheet = xlPackage.Workbook.Worksheets.First();
				foreach (var key in errorKeys)
				{
					worksheet.Cells[message.Settings.InformationRowPosition, key].Style.Fill.BackgroundColor.SetColor(Color.Red);
				}			
				xlPackage.Save();
			}
			return message;
		};
	}
}