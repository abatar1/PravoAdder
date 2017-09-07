using System;
using System.IO;
using PravoAdder.Domain;

namespace PravoAdder.Readers
{
    public abstract class ExcelReader
    {
        public abstract ExcelTable Read(Settings settings);

        protected static string FormatCell(object cell)
        {
            var cellString = cell?.ToString();
            if (!(cell is DateTime)) return cellString;

            return $"{(DateTime) cell:yyyy-MM-dd}";
        }

        protected static FileInfo GetFileInfo(string name)
        {
            var info = new FileInfo(name);
            if (!info.Name.Contains(".xlsx")) info = new FileInfo(name + ".xlsx");
            if (!info.Exists) throw new FileNotFoundException($"File {info.Name} not found!");
            return info;
        }
    }
}