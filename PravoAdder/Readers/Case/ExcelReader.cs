using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using PravoAdder.Domain;

namespace PravoAdder.Readers
{
    public class ExcelReader : TemplateTableReader
    {
	    protected override FileInfo GetFileInfo(string name, params string[] stub)
	    {
			var extentions = new[] {".xlsx", ".xlsb", ".xlsm"};
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
					.Where(c => c.Value != null)
					.ToArray();
                var colorColumnsPositions = infoRow
                    .Where(c => settings.AllowedColors.Contains(c.Style.Fill.BackgroundColor.Rgb))
                    .Select(c => c.Start.Column)
                    .ToArray();
                var infoRowContent = infoRow
                    .Where(c => colorColumnsPositions.Contains(c.Start.Column))
                    .Select(c => FormatCell(c.Value) ?? string.Empty)
                    .Zip(colorColumnsPositions, (value, key) => new {value, key})
                    .ToDictionary(key => key.key, value => new FieldAddress(value.value));

                var table = new List<Dictionary<int, FieldAddress>>();
	            var startPosition = args.RowNum != 0 ? args.RowNum : settings.DataRowPosition;
				for (var rowNum = startPosition; rowNum <= totalRows; rowNum++)
                {
                    var row = new List<string>();
                    for (var columnNum = 1; columnNum <= totalColumns; columnNum++)
                    {
                        var cell = worksheet.Cells[rowNum, columnNum].Value?.ToString();
                        row.Add(cell);
                    }
                    var coloredRow = row
                        .Zip(Enumerable.Range(1, totalColumns), (value, index) => new {value, index})
                        .Where(z => colorColumnsPositions.Contains(z.index))
                        .ToDictionary(key => key.index, value => new FieldAddress(value.value));
                    table.Add(coloredRow);
                }
                return new Table(table.Select(row => new Row(row)), new Row(infoRowContent));
            }
        }
    }
}