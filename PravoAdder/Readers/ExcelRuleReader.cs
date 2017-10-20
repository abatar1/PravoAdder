using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OfficeOpenXml;
using PravoAdder.Domain;

namespace PravoAdder.Readers
{
	public class ExcelRuleReader : TemplateTableReader
	{
		protected override FileInfo GetFileInfo(string name, params string[] stub)
		{
			var extentions = new[] { ".xlsx", ".xlsb", ".xlsm" };
			return base.GetFileInfo(name, extentions);
		}

		public override Table Read(ApplicationArguments args, Settings settings)
		{
			var info = GetFileInfo(args.SourceFileName);

			using (var xlPackage = new ExcelPackage(info))
			{
				var worksheet = xlPackage.Workbook.Worksheets.First();

				var totalRows = worksheet.Dimension.End.Row;
				var totalColumns = worksheet.Dimension.End.Column;

				var infoRow = worksheet
					.Cells[settings.InformationRowPosition, 1, settings.InformationRowPosition, totalColumns]
					.Where(cell => cell.Value != null)
					.ToArray();
				var colorColumnsPositions = infoRow
					.Where(cell => settings.AllowedColors.Contains(cell.Style.Fill.BackgroundColor.Rgb))
					.Select(cell => cell.Start.Column)
					.ToArray();
				var infoRowContent = infoRow
					.Where(cell => colorColumnsPositions.Contains(cell.Start.Column))
					.Select(cell => FormatCell(cell.Value) ?? string.Empty)
					.Zip(colorColumnsPositions, (value, key) => new { value, key })
					.ToDictionary(key => key.key, value => new FieldAddress(value.value));

				var table = new List<IDictionary<int, string>>();
				for (var rowNum = settings.DataRowPosition + args.RowNum - 1; rowNum <= totalRows; rowNum++)
				{
					var row = new List<string>();
					for (var columnNum = 1; columnNum <= totalColumns; columnNum++)
					{
						var cell = worksheet.Cells[rowNum, columnNum].Value?.ToString();
						row.Add(cell);
					}
					var coloredRow = row
						.Zip(Enumerable.Range(1, totalColumns), (value, index) => new { value, index })
						.Where(z => colorColumnsPositions.Contains(z.index))
						.ToDictionary(key => key.index, value => value.value);
					table.Add(coloredRow);
				}

				var keyIndex = GetIndexByName(infoRowContent, "Номер дела");
				var responseIndex = GetIndexByName(infoRowContent, "Ответственный");
				var innIndex = GetIndexByName(infoRowContent, "ИНН");
				var idParticipantIndex = GetIndexByName(infoRowContent, "ID контрагента");

				var rgx = new Regex(@"^А\d{2}-[0-9]+/20\d{2}");
				var groupedTable = table
					.GroupBy(row => row.First(cell => cell.Key == keyIndex).Value)
					.Where(row => row.Key != null && rgx.IsMatch(row.Key))
					.Select(row => row.Last()
						.Select(cell => 
						{
							if (cell.Key == responseIndex)
							{
								if (cell.Value == null || cell.Value == "Общее")
								{
									return new KeyValuePair<int, string>(cell.Key, "Неназначенный Ответственный");
								}
							}
							return cell;
						})
						.ToDictionary(key => key.Key, value => 
						{
							var isDate = DateTime.TryParseExact(value.Value, "dd.MM.yyyy h:mm:ss", System.Globalization.CultureInfo.InvariantCulture,
								System.Globalization.DateTimeStyles.None, out var dateTime);
							return new FieldAddress(FormatCell(isDate ? dateTime : (object) value.Value));
						}))
						.Select(row => new Row(row))
					.ToList();
				return new Table(groupedTable, new Row(infoRowContent));
			}
		}

		private static int GetIndexByName(Dictionary<int, FieldAddress> row, string name)
		{
			return row.First(x => x.Value.FieldName == name).Key;
		}
	}
}
