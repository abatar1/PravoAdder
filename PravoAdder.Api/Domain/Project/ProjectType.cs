using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class ProjectType : DatabaseEntityItem
	{
		public VisualBlock VisualBlock { get; set; }
		public List<VisualBlock> VisualBlocks { get; set; }
		public string Abbreviation { get; set; }
	}
}
