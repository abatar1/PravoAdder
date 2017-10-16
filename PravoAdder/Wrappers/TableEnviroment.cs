using System;
using NLog;
using PravoAdder.Domain;
using PravoAdder.Readers;

namespace PravoAdder.Wrappers
{
	public class TableEnviroment
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		private static Table _table;
		public static Table Table
		{
			get
			{
				if (_table == null) throw new ArgumentNullException($"Table doesn't initialized");
				return _table;
			}
			private set => _table = value;
		}

		public static void Initialize(ApplicationArguments args, Settings settings)
		{
			TemplateTableReader tableReader;
			switch (args.ReaderMode)
			{
				case ReaderMode.Excel:
					tableReader = new ExcelReader();
					Table = tableReader.Read(args, settings);
					break;
				case ReaderMode.XmlMap:
					tableReader = new XmlMappingReader();
					Table = tableReader.Read(args, settings);
					break;
				case ReaderMode.ExcelRule:
					tableReader = new ExcelRuleReader();
					Table = tableReader.Read(args, settings);
					break;
				case ReaderMode.ExcelReference:
					tableReader = new ExcelReferenceReader();
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
