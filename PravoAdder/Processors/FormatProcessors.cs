using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PravoAdder.Api.Domain;
using PravoAdder.Domain;
using PravoAdder.Helpers;

namespace PravoAdder.Processors
{
	public class FormatProcessors
	{
		public Func<EngineMessage, EngineMessage> AnalyzeHeader = message =>
		{
			var types = ApiRouter.ProjectTypes.GetMany(message.Authenticator);
			var blockTypes = new Dictionary<string, List<(string, List<string>)>>();
			var errorKeys = new List<int>();
			foreach (var cell in message.Table.Header)
			{
				if (cell.Value.IsSystem)
				{
					var systemHeaderName = typeof(HeaderBlockInfo).GetProperties()
						.Select(p => p.GetAttribute<FieldNameAttribute>().FieldNames)
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
			using (var xlPackage = new ExcelPackage(new FileInfo(message.Settings.SourceName + ".xlsx")))
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

		private static void ProcessHeader(EngineMessage message, Func<string, string> cellProcessor)
		{
			using (var xlPackage = new ExcelPackage(new FileInfo(message.Settings.SourceName + ".xlsx")))
			{
				var worksheet = xlPackage.Workbook.Worksheets.First();
				var totalColumns = worksheet.Dimension.End.Column;

				for (var columnNum = 1; columnNum <= totalColumns; columnNum++)
				{
					var cellValue = worksheet.Cells[message.Settings.InformationRowPosition, columnNum].Text
						.Replace("Summary", "")
						.Replace("/", "")
						.Trim();
					var processedValue = cellProcessor(cellValue);
					if (processedValue == null) continue;

					worksheet.Cells[message.Settings.InformationRowPosition, columnNum].Style.Fill.PatternType = ExcelFillStyle.Solid;
					worksheet.Cells[message.Settings.InformationRowPosition, columnNum].Style.Fill.BackgroundColor
						.SetColor(Color.Yellow);
					worksheet.Cells[message.Settings.InformationRowPosition, columnNum].Value = processedValue;
				}
				xlPackage.Save();
			}			
		}

		public Func<EngineMessage, EngineMessage> Expenses = message =>
		{
			ProcessHeader(message, cellValue =>
			{
				switch (cellValue)
				{
					case "Date":
						return new FieldAddress("temp", cellValue).ToString();
					case "Case":
						return new FieldAddress("Summary", "Case Name").ToString();
					case "Total":
						return new FieldAddress("temp", "Amount").ToString();
					case "Description":
						return new FieldAddress("temp", "Expense").ToString();
					case "Client":
						return new FieldAddress("temp", cellValue).ToString();
					default:
						return null;
				}
			});

			return message;
		};

		public Func<EngineMessage, EngineMessage> Case = message =>
		{
			ProcessHeader(message, cellValue =>
			{
				switch (cellValue)
				{
					case "Case Name":
						return new FieldAddress("Summary", "Case Name").ToString();
					case "CaseId":
						return new FieldAddress("Summary", "Files").ToString();
					case "Atty-Div":
						return new FieldAddress("Summary", "Assignee").ToString();
					case "Liability":
						return new FieldAddress("Summary", "Practice Area").ToString();
					case "Billable Client":
						return new FieldAddress("Summary", "Practice Area").ToString();
					default:
						return null;
				}
			});

			return message;
		};

		public Func<EngineMessage, EngineMessage> Contact = message =>
		{
			Func<string, string> processor = str => str;
			if (message.Settings.ParticipantType == ParticipantType.PersonTypeName)
			{
				processor = cellValue =>
				{
					switch (cellValue)
					{					
						case "Company":
							return new FieldAddress("Summary", "Company").ToString();
						case "First Name":
							return new FieldAddress("Summary", "First Name").ToString();
						case "Last Name":
							return new FieldAddress("Summary", "Last Name").ToString();
						case "Job Title":
							return new FieldAddress("Summary", "Job Title").ToString();
						case "Web Page":
							return new FieldAddress("Summary", "Site").ToString();
						case "E-mail Address":
							return new FieldAddress("Summary", "Email").ToString();

						case "Title":
							return new FieldAddress("More", "Title").ToString();
						case "Business Street":
							return new FieldAddress("More", "Work Address").ToString();
						case "Business City":
							return new FieldAddress("More", "Work City").ToString();
						case "Business State":
							return new FieldAddress("More", "Work State").ToString();
						case "Business CountryRegion":
							return new FieldAddress("More", "Work Country").ToString();
						case "Business Postal Code":
							return new FieldAddress("More", "Work Zip").ToString();
						case "Business Phone":
							return new FieldAddress("More", "Work Phone").ToString();
						case "Home Street":
							return new FieldAddress("More", "Home Address").ToString();
						case "Home City":
							return new FieldAddress("More", "Home City").ToString();
						case "Home State":
							return new FieldAddress("More", "Home State").ToString();
						case "Home CountryRegion":
							return new FieldAddress("More", "Home Country").ToString();
						case "Home Postal Code":
							return new FieldAddress("More", "Home Zip").ToString();
						case "Home Phone":
							return new FieldAddress("More", "Home Phone").ToString();
						case "Mobile Phone":
							return new FieldAddress("More", "Cellphone").ToString();
						case "Business Fax":
							return new FieldAddress("More", "FAX").ToString();
						default:
							return null;
					}
				};
			}
			else if (message.Settings.ParticipantType == ParticipantType.CompanyTypeName)
			{
				processor = cellValue =>
				{
					switch (cellValue)
					{
						case "Company":
							return new FieldAddress("Summary", "Company").ToString();
						case "Web Page":
							return new FieldAddress("Summary", "Site").ToString();
						case "E-mail Address":
							return new FieldAddress("Summary", "Email").ToString();
						case "Business Street":
							return new FieldAddress("Summary", "Address").ToString();

						case "Business City":
							return new FieldAddress("More", "City").ToString();
						case "Business State":
							return new FieldAddress("More", "State").ToString();
						case "Business CountryRegion":
							return new FieldAddress("More", "Country").ToString();
						case "Business Postal Code":
							return new FieldAddress("More", "Zip").ToString();
						case "Business Phone":
							return new FieldAddress("More", "Phone").ToString();
						case "Mobile Phone":
							return new FieldAddress("More", "Cellphone").ToString();
						case "Business Fax":
							return new FieldAddress("More", "FAX").ToString();
						default:
							return null;
					}
				};
			}

			ProcessHeader(message, processor);
			return message;
		};

	}
}
