using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace PravoAdder.Reader
{
    public class ExcelReader
    {
        public static IEnumerable<IDictionary<int, string>> ReadDataFromTable(string filename, int dataRowNum = 4)
        {
            using (var xlPackage = new ExcelPackage(new FileInfo(filename)))
            {
                var myWorksheet = xlPackage.Workbook.Worksheets.First();
                var totalRows = myWorksheet.Dimension.End.Row;
                var totalColumns = myWorksheet.Dimension.End.Column;

                for (var rowNum = dataRowNum; rowNum <= totalRows; rowNum++)
                {
                    yield return myWorksheet
                        .Cells[rowNum, 2, rowNum, totalColumns]
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
