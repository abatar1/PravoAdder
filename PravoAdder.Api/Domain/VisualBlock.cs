using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class VisualBlock
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public List<VisualBlockLine> Lines { get; set; }
		public bool IsRepeatable { get; set; }

		public override string ToString() => Name;
	}
}
