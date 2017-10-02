using System;
using System.Collections.Generic;
using PravoAdder.Wrappers;

namespace PravoAdder
{
	public class EngineRequest : EngineResponse
	{
		public DatabaseEnviromentWrapper Migrator { get; set; }
		public BlockReaderWrapper BlockReader { get; set; }
		public IDictionary<int, string> ExcelRow { get; set; }		
		public int Index { get; set; }
		public DateTime Date { get; set; }
	}
}

