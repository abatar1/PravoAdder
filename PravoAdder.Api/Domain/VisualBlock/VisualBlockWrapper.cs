using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class VisualBlockWrapper
	{
		public VisualBlockModel VisualBlock { get; set; }
		public List<VisualBlockModel> VisualBlocks { get; set; }
		public List<VisualBlockModel> Blocks { get; set; }
		public List<VisualBlockModel> MetadataOfBlocks { get; set; }
		public int Order { get; set; }
	}
}
