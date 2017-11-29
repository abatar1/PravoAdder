using System;
using NLog;
using PravoAdder.Domain;
using PravoAdder.Readers;

namespace PravoAdder.Wrappers
{
	public class TableEnviroment
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		public static Table Create(string filename, ApplicationArguments args, Settings settings)
		{
			TemplateTableReader tableReader;
			switch (args.ReaderMode)
			{
				case ReadingMode.Excel:
					tableReader = new ExcelReader(filename);
					return tableReader.Read(args, settings);
				case ReadingMode.XmlMap:
					tableReader = new XmlMappingReader();
					return tableReader.Read(args, settings);
				case ReadingMode.ExcelRule:
					tableReader = new ExcelRuleReader();
					return tableReader.Read(args, settings);
				case ReadingMode.ExcelReference:
					tableReader = new ExcelReferenceReader();
					return tableReader.Read(args, settings);
				case ReadingMode.ExcelSplit:
					tableReader = new ExcelSplitTables();
					return tableReader.Read(args, settings);
				default:
					var message = $"Типа блоков {args.ReaderMode} не существует.";
					Logger.Error(message);
					throw new ArgumentException(message);
			}
		}
	}
}
