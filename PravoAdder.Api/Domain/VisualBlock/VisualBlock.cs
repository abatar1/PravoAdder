using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class VisualBlock : DatabaseEntityItem
	{
		public string VisualBlockId { get; set; }
		public string ProjectId { get; set; }
		public List<VisualBlockLine> Lines { get; set; }
		public bool IsRepeatable { get; set; }
		public string NameInConstructor { get; set; }
		public int Order { get; set; }
		public int FrontOrder { get; set; }
	}
}
