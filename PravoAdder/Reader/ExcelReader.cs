using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace PravoAdder.Reader
{
    public class ExcelReader
    {
        public static IEnumerable<IDictionary<int, string>> ReadDataFromTable(string filename, int dataRowPosition, int infoRowPosition)
        {
            var info = new FileInfo(filename);
            if (!info.Exists) throw new FileNotFoundException($"File {filename} not found!");

            using (var xlPackage = new ExcelPackage(info))
            {
                var worksheet = xlPackage.Workbook.Worksheets.First();
                var totalRows = worksheet.Dimension.End.Row;
                var totalColumns = worksheet.Dimension.End.Column;

                var colorColumnsPositions = worksheet
                    .Cells[infoRowPosition, 1, infoRowPosition, totalColumns]
                    .Where(c => c.Style.Fill.BackgroundColor.Rgb != "")
                    .Select(c => c.Start.Column);

                for (var rowNum = dataRowPosition; rowNum <= totalRows; rowNum++)
                {
                    yield return worksheet                        
                        .Cells[rowNum, 1, rowNum, totalColumns]
                        .Where(c => colorColumnsPositions.Contains(c.Start.Column))
                        .Select(c => FormatCell(c.Value) ?? string.Empty)
                        .Zip(Enumerable.Range(1, totalColumns - 1), (value, key) => new {value, key})                       
                        .ToDictionary(key => key.key, value => value.value);
                }
            }
        }

        private static string FormatCell(object cell)
        {
            var cellString = cell?.ToString();
            if (!(cell is DateTime)) return cellString;
            
            var separatorIndex = cellString.IndexOf(" ", StringComparison.Ordinal);
            if (separatorIndex > 0) cellString = cellString.Substring(0, separatorIndex);

            return string.Join("-", cellString.Split('.').Reverse());
        }
    }
}
