using PravoAdder.Api.Domain;
using PravoAdder.Domain;

namespace PravoAdder
{
	public class EngineResponse
	{
		public HeaderBlockInfo HeaderBlock { get; set; }
		public DatabaseEntityItem Item { get; set; }
	}
}
