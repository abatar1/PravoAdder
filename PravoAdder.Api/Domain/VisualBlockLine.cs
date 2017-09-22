using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class VisualBlockLine
	{
		public string Id { get; set; }
		public List<VisualBlockField> Fields { get; set; }
		public LineType LineType { get; set; }
	}
}
