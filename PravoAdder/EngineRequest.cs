using PravoAdder.TableEnviroment;
using PravoAdder.Wrappers;

namespace PravoAdder
{
	public class EngineRequest : EngineResponse
	{
		public DatabaseEnviromentWrapper Migrator { get; set; }
		public BlockReader BlockReader { get; set; }
		public Row Row { get; set; }		
		public int Index { get; set; }
	}
}

