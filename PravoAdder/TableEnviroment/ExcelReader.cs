using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace PravoAdder.TableEnviroment
{
    public class ExcelReader : TableReader
    {
	    protected override FileInfo GetFileInfo(string name, params string[] stub)
	    {
		    var extentions = new[] {".xlsx", ".xlsb", ".xlsm"};
		    return base.GetFileInfo(name, extentions);
	    }

	    public override Table Read(TableSettings settings)
        {
            var info = GetFileInfo(settings.SourceFileName);

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
                var headerRowContent = infoRow
                    .Where(c => colorColumnsPositions.Contains(c.Start.Column))
                    .Select(c => FormatCell(c.Value) ?? string.Empty)
                    .Zip(colorColumnsPositions, (value, key) => new {value, key})
                    .ToDictionary(key => key.key, value => new FieldInfo(value.value));

                var table = new List<Row>();
                for (var rowNum = settings.DataRowPosition + settings.StartRow - 1; rowNum <= totalRows; rowNum++)
                {
                    var line = new List<string>();
                    for (var columnNum = 1; columnNum <= totalColumns; columnNum++)
                    {
                        var cell = worksheet.Cells[rowNum, columnNum].Value?.ToString();
                        line.Add(cell);
                    }
                    var coloredLine = line
                        .Zip(Enumerable.Range(1, totalColumns), (value, index) => new {value, index})
                        .Where(z => colorColumnsPositions.Contains(z.index))
                        .ToDictionary(key => key.index, value => new FieldInfo(value.value));
					var row = new Row(coloredLine);
                    table.Add(row);
                }
                return new Table(table, new Row(headerRowContent));
            }
        }
    }
}