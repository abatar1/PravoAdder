using System;
using System.IO;
using System.Linq;
using PravoAdder.Domain;

namespace PravoAdder.Readers
{
    public abstract class TemplateTableReader
    {
        public abstract Table Read(ApplicationArguments arg, Settings settings);

        protected virtual string FormatCell(object cell)
        {
            var cellString = cell?.ToString();
            if (!(cell is DateTime)) return cellString;

            return $"{(DateTime) cell:yyyy-MM-dd}";
        }

	    protected virtual FileInfo GetFileInfo(string name, params string[] extentions)
	    {
			if (Path.HasExtension(name)) return new FileInfo(name);
		    return extentions
				.Select(extention => new FileInfo(name + extention))
				.FirstOrDefault(info => info.Exists);
	    }
    }
}