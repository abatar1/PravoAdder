using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;
using PravoAdder.Domain;

namespace PravoAdder.Readers.Color
{
    public class ColorExcelReader : TableReader
    {
        public override Table Read(Settings settings)
        {
            var info = GetFileInfo(settings.SourceFileName, ".xslx");

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
                var infoRowContent = infoRow
                    .Where(c => colorColumnsPositions.Contains(c.Start.Column))
                    .Select(c => FormatCell(c.Value) ?? string.Empty)
                    .Zip(colorColumnsPositions, (value, key) => new {value, key})
                    .ToDictionary(key => key.key, value => value.value);

                var table = new List<IDictionary<int, string>>();
                for (var rowNum = settings.DataRowPosition + settings.StartRow - 1; rowNum <= totalRows; rowNum++)
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
                        .ToDictionary(key => key.index, value => value.value);
                    table.Add(coloredRow);
                }
                return new Table(table, infoRowContent);
            }
        }
    }
}