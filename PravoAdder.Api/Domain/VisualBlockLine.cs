using System;
using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class VisualBlockLine : ICloneable
	{
		public string Id { get; set; }
		public string BlockLineId { get; set; }
		public List<VisualBlockField> Fields { get; set; }
		public LineType LineType { get; set; }

		public object Clone()
		{
			return new VisualBlockLine
			{
				Id = Id,
				BlockLineId = BlockLineId,
				Fields = new List<VisualBlockField>(Fields),
				LineType = LineType
			};
		}
	}
}
