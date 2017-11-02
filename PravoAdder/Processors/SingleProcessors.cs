using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NLog;
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

		public static Func<EngineMessage, EngineMessage> CreateProjectField = message =>
		{
			var projectField = (ProjectField) message.GetCreatable();
			var result = ApiRouter.ProjectFields.CreateProjectField(message.Authenticator, projectField);
			message.Item = result;
			return message;
		};

		public static Func<EngineMessage, EngineMessage> ProcessCount = message =>
		{
			if (message.Item == null) return message;
			message.Counter.ProcessCount(message.Count, message.Total, message.Item, 70);
			return message;
		};

		public static Func<EngineMessage, EngineMessage> CreateTask = message =>
		{
			var task = (Api.Domain.Task) message.GetCreatable();
			ApiRouter.Task.Create(message.Authenticator, task);
			if (task.IsArchive) ApiRouter.Projects.ArchiveProject(message.Authenticator, task.Project.Id);
			return message;
		};

		private static List<VisualBlock> _visualBlocks;
		private static List<LineType> _lineTypes;

		public static Func<EngineMessage, EngineMessage> AddVisualBlockRow = message =>
		{
			if (_visualBlocks == null) _visualBlocks = ApiRouter.VisualBlock.Get(message.Authenticator);
			var visualBlock = _visualBlocks.FirstOrDefault(vb => vb.Name == message.GetValueFromRow("Data block"));
			if (visualBlock == null) return null;

			if (_lineTypes == null)
				_lineTypes = ApiRouter.VisualBlock.GetLineTypes(message.Authenticator);

			var projectField = ApiRouter.ProjectFields.Get(message.Authenticator, message.GetValueFromRow("Field name")).FirstOrDefault();
			if (projectField == null) return null;

			var tagNamingRule = new Regex("[^a-яA-Яa-zA-Z0-9_]");
			var newLine = new VisualBlockLine
			{
				LineType = _lineTypes.First(t => t.Name == message.GetValueFromRow("Row")),
				Fields = new List<VisualBlockField>
				{
					new VisualBlockField
					{
						IsRequired = bool.Parse(message.GetValueFromRow("Required")),
						Tag = tagNamingRule.Replace(message.GetValueFromRow("Tag"), "_"),
						Width = int.Parse(message.GetValueFromRow("Width")),
						ProjectField = projectField
					}
				},
				Order = visualBlock.Lines.Count				
			};
			visualBlock.Lines.Add(newLine);

			var result = ApiRouter.VisualBlock.Update(message.Authenticator, visualBlock);
			message.Item = result;
			return message;
		};

		public static Func<EngineMessage, EngineMessage> CreateExcelRow = message =>
		{
			const string pageName = "Content";
			var newTable = new FileInfo("output.xlsx");				
			var pck = new ExcelPackage(newTable);
			var head = typeof(HeaderBlockInfo).GetProperties();

			ExcelWorksheet worksheet = null;
			if (!newTable.Exists)
			{
				if (!HeaderBlockInfo.Languages.ContainsKey(message.Args.Language))
				{
					message.IsFinal = true;
					return message;
				}
				var lKey = HeaderBlockInfo.Languages[message.Args.Language];
				worksheet = pck.Workbook.Worksheets.Add(pageName);
				var alphabet = Enumerable.Range(0, 26).Select(i => Convert.ToChar('A' + i));			

				foreach (var p in head.Zip(alphabet, (info, c) => new {Info = info.LoadAttribute<FieldNameAttribute>(), Letter = c}))
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
			if (worksheet == null) worksheet = pck.Workbook.Worksheets.First(w => w.Name == pageName);

			var project = ApiRouter.Projects.GetProject(message.Authenticator, message.Item.Id);
			var projectProperties = typeof(Project).GetProperties();

			foreach (var p in head.Zip(Enumerable.Range(1, head.Length + 1), (info, i) => new {Info = info, Count = i}))
			{
				var projectProperty = projectProperties.FirstOrDefault(prop => prop.Name == p.Info.Name);
				if (projectProperty == null) continue;

				var value = projectProperty.PropertyType.BaseType == typeof(DatabaseEntityItem)
					? ((DatabaseEntityItem) projectProperty.GetValue(project))?.Name
					: Convert.ChangeType(projectProperty.GetValue(project), projectProperty.PropertyType)?.ToString();
				worksheet.Cells[message.Count + 2, p.Count].Value = value;
			}
			//worksheet.Cells[message.Count + 2, 1].Value = project.ProjectFolder?.Name;
			//worksheet.Cells[message.Count + 2, 2].Value = project.ProjectGroup?.Name;
			//worksheet.Cells[message.Count + 2, 3].Value = project.Name;
			//worksheet.Cells[message.Count + 2, 4].Value = project.CasebookNumber;
			pck.Save();

			message.Item = project;
			return message;
		};

		public static Func<EngineMessage, EngineMessage> AnalyzeHeader = message =>
		{
			var types = ApiRouter.ProjectTypes.GetProjectTypes(message.Authenticator);
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