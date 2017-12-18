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
			switch (settings.ReadingMode)
			{
				case ReadingMode.Excel:
					tableReader = new ExcelReader(filename);
					return tableReader.Read(settings);
				case ReadingMode.XmlMap:
					tableReader = new XmlMappingReader();
					return tableReader.Read(settings);
				case ReadingMode.ExcelRule:
					tableReader = new ExcelRuleReader();
					return tableReader.Read(settings);
				case ReadingMode.ExcelReference:
					tableReader = new ExcelReferenceReader();
					return tableReader.Read(settings);
				case ReadingMode.ExcelSplit:
					tableReader = new ExcelSplitTables();
					return tableReader.Read(settings);
				default:
					var message = $"Типа блоков {settings.ReadingMode} не существует.";
					Logger.Error(message);
					throw new ArgumentException(message);
			}
		}
	}
}
