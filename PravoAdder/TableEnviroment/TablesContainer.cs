using System;
using NLog;
using PravoAdder.Domain;

namespace PravoAdder.TableEnviroment
{
	public class TablesContainer
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

		public static void Initialize(TableSettings settings)
		{
			TableReader tableReader;
			switch (settings.BlockReadingMode)
			{
				case ReaderMode.Excel:
					tableReader = new ExcelReader();
					Table = tableReader.Read(settings);
					break;
				case ReaderMode.XmlMap:
					tableReader = new XmlWithMappingReader();
					Table = tableReader.Read(settings);
					break;
				case ReaderMode.ExcelReference:
					tableReader = new ExcelWithReferenceReader();
					Table = tableReader.Read(settings);
					break;
				default:
					var message = $"Типа блоков {settings.BlockReadingMode} не существует.";
					Logger.Error(message);
					throw new ArgumentException(message);
			}
		}
	}
}
