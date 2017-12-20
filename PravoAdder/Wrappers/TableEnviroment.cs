using System;
using NLog;
using PravoAdder.Domain;
using PravoAdder.Readers;

namespace PravoAdder.Wrappers
{
	public class TableEnviroment
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		public static Table Create(string filename, Settings settings)
		{
			TemplateTableReader tableReader;
			switch (settings.DataReadingMode)
			{
				case DataReadingMode.Excel:
					tableReader = new ExcelReader(filename);
					return tableReader.Read(settings);
				case DataReadingMode.XmlMap:
					tableReader = new XmlMappingReader();
					return tableReader.Read(settings);
				case DataReadingMode.ExcelRule:
					tableReader = new ExcelRuleReader();
					return tableReader.Read(settings);
				case DataReadingMode.ExcelReference:
					tableReader = new ExcelReferenceReader();
					return tableReader.Read(settings);
				case DataReadingMode.ExcelSplit:
					tableReader = new ExcelSplitTables();
					return tableReader.Read(settings);
				default:
					var message = $"Типа блоков {settings.DataReadingMode} не существует.";
					Logger.Error(message);
					throw new ArgumentException(message);
			}
		}
	}
}
