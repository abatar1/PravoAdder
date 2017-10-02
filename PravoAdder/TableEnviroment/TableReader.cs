using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PravoAdder.TableEnviroment
{
    public abstract class TableReader
    {
        public abstract Table Read(TableSettings settings);

		private static string FormatDate(DateTime date) => $"{date:yyyy-MM-dd}";

		protected virtual string FormatCell(object cell)
        {
            var cellString = cell?.ToString();
            if (!(cell is DateTime)) return cellString;

	        return FormatDate((DateTime) cell);
        }

	    protected virtual string FormatCell(string cell)
	    {
		    if (cell == null) return null;

		    if (DateTime.TryParseExact(cell, "dd.MM.yyyy H:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None,
			    out var dateTime))
			    return FormatDate(dateTime);
			return cell;
	    }

		protected virtual FileInfo GetFileInfo(string name, params string[] extentions)
	    {
		    return extentions
				.Select(extention => new FileInfo(name + extention))
				.FirstOrDefault(info => info.Exists);
	    }
    }
}