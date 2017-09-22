using System.Collections.Generic;
using PravoAdder.Controllers;

namespace PravoAdder
{
	public class EngineRequest
	{
		public EngineRequest(MigrationProcessController migrator, BlockReaderController reader, IDictionary<int, string> excelRow)
		{
			Migrator = migrator;
			BlockReader = reader;
			ExcelRow = excelRow;
		}

		public MigrationProcessController Migrator { get; }
		public BlockReaderController BlockReader { get; }
		public IDictionary<int, string> ExcelRow { get; }
	}
}
