using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class VisualBlock : DatabaseEntityItem
	{
		public List<VisualBlockLine> Lines { get; set; }
		public bool IsRepeatable { get; set; }

		public override string ToString() => Name;
	}
}
