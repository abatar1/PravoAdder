using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class VisualBlockWrapper
	{
		public VisualBlock VisualBlock { get; set; }
		public List<VisualBlock> VisualBlocks { get; set; }
		public int Order { get; set; }
	}
}
