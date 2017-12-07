using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class VisualBlockLine : DatabaseEntityItem
	{
		public string BlockLineId { get; set; }
		public List<VisualBlockField> Values { get; set; }
		public int Order { get; set; }
	}
}
