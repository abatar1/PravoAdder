using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class ProjectType : DatabaseEntityItem
	{
		public VisualBlockModel VisualBlock { get; set; }
		public List<VisualBlockModel> VisualBlocks { get; set; }
		[Required]
		public string Abbreviation { get; set; }
		public bool IsArchive { get; set; }
	}
}
