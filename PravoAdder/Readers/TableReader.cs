using System;
using System.IO;
using PravoAdder.Domain;

namespace PravoAdder.Readers
{
    public abstract class TableReader
    {
        public abstract Table Read(Settings settings);

        protected static string FormatCell(object cell)
        {
            var cellString = cell?.ToString();
            if (!(cell is DateTime)) return cellString;

            return $"{(DateTime) cell:yyyy-MM-dd}";
        }

	    protected static FileInfo GetFileInfo(string name, string extention)
	    {
		    var info = new FileInfo(name);
		    if (info.Extension != extention) info = new FileInfo(name + extention);
		    return !info.Exists ? null : info;
	    }
    }
}