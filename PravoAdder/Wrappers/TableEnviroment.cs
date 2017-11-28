using System;
using NLog;
using PravoAdder.Domain;
using PravoAdder.Readers;

namespace PravoAdder.Wrappers
{
	public class TableEnviroment
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
		public static Table Table { get; private set; }

		public static void Initialize(ApplicationArguments args, Settings settings)
		{
			TemplateTableReader tableReader;
			switch (args.ReaderMode)
			{
				case ReadingMode.Excel:
					tableReader = new ExcelReader(args.SourceFileName);
					Table = tableReader.Read(args, settings);
					break;
				case ReadingMode.XmlMap:
					tableReader = new XmlMappingReader();
					Table = tableReader.Read(args, settings);
					break;
				case ReadingMode.ExcelRule:
					tableReader = new ExcelRuleReader();
					Table = tableReader.Read(args, settings);
					break;
				case ReadingMode.ExcelReference:
					tableReader = new ExcelReferenceReader();
					Table = tableReader.Read(args, settings);
					break;
				case ReadingMode.ExcelSplit:
					tableReader = new ExcelSplitTables();
					Table = tableReader.Read(args, settings);
					break;
				default:
					var message = $"Типа блоков {args.ReaderMode} не существует.";
					Logger.Error(message);
					throw new ArgumentException(message);
			}
		}
	}
}
