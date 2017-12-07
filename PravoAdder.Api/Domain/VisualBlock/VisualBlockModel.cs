using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class VisualBlockModel : DatabaseEntityItem
	{
		public string VisualBlockId { get; set; }
		public string ProjectId { get; set; }		
		public bool IsRepeatable { get; set; }
		public string NameInConstructor { get; set; }
		public int Order { get; set; }
		public int FrontOrder { get; set; }
		public List<VisualBlockLineModel> Lines { get; set; }
	}
}
