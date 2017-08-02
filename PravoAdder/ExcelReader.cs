using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace PravoAdder
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
                        .Select(c => c.Value?.ToString() ?? string.Empty)
                        .Zip(Enumerable.Range(2, totalColumns), (value, key) => new {value, key})                       
                        .ToDictionary(key => key.key, value => value.value);
                }
            }
        }       
    }
}
