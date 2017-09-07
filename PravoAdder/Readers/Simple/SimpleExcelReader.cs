using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;
using PravoAdder.Domain;

namespace PravoAdder.Readers
{
    public class SimpleExcelReader : ExcelReader
    {
        public override ExcelTable Read(Settings settings)
        {
            var info = GetFileInfo(settings.ExcelFileName);

            var table = new List<IDictionary<int, string>>();
            using (var xlPackage = new ExcelPackage(info))
            {
                var worksheet = xlPackage.Workbook.Worksheets.First();
                var totalRows = worksheet.Dimension.End.Row;
                var totalColumns = worksheet.Dimension.End.Column;

                for (var rowNum = settings.DataRowPosition; rowNum <= totalRows; rowNum++)
                    table.Add(worksheet
                        .Cells[rowNum, 1, rowNum, totalColumns]
                        .Select(c => FormatCell(c.Value) ?? string.Empty)
                        .Zip(Enumerable.Range(1, totalColumns), (value, key) => new {value, key})
                        .ToDictionary(key => key.key, value => value.value));
            }
            return new ExcelTable(table, new Dictionary<int, string>());
        }
    }
}