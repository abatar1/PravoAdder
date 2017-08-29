using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using PravoAdder.Domain;

namespace PravoAdder.Readers
{
	public class ColorExcelReader : ExcelReader
	{
		private static Dictionary<int, string> ToColoredDictionary(ExcelRange range, ICollection<int> colorColumnsPositions)
		{
			return range
				.Where(c => colorColumnsPositions.Contains(c.Start.Column))
				.Select(c => FormatCell(c.Value) ?? string.Empty)
				.Zip(colorColumnsPositions, (value, key) => new {value, key})
				.ToDictionary(key => key.key, value => value.value);
		}

		public override ExcelTable Read(Settings settings)
		{
			var info = new FileInfo(settings.ExcelFileName);
			if (!info.Exists) throw new FileNotFoundException($"File {info.Name} not found!");

			var table = new List<IDictionary<int, string>>();
			IDictionary<int, string> infoRowContent;
			using (var xlPackage = new ExcelPackage(info))
			{
				var worksheet = xlPackage.Workbook.Worksheets.First();

				var totalRows = worksheet.Dimension.End.Row;
				var totalColumns = worksheet.Dimension.End.Column;

				var infoRow = worksheet
					.Cells[settings.InformationRowPosition, 1, settings.InformationRowPosition, totalColumns];
				var colorColumnsPositions = infoRow
					.Where(c => settings.AllowedColors.Contains(c.Style.Fill.BackgroundColor.Rgb))
					.Select(c => c.Start.Column)
					.ToList();
				infoRowContent = ToColoredDictionary(infoRow, colorColumnsPositions);

				for (var rowNum = settings.DataRowPosition; rowNum <= totalRows; rowNum++)
				{
					table.Add(ToColoredDictionary(worksheet
						.Cells[rowNum, 1, rowNum, totalColumns], colorColumnsPositions));
				}
			}
			return new ExcelTable(table, infoRowContent);
		}
	}
}