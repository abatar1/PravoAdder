using System;
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
	}
}