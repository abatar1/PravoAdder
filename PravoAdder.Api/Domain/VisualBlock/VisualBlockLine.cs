using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace PravoAdder.Api.Domain
{
	public class VisualBlockLine : DatabaseEntityItem, ICreatable
	{		
		public string BlockLineId { get; set; }
		public List<VisualBlockField> Fields { get; set; }
		public List<VisualBlockParticipantField> Values { get; set; }
		public LineType LineType { get; set; }
		public int Order { get; set; }

		[JsonIgnore]
		public bool IsSimple => LineType.SysName == "Simple";

		[JsonIgnore]
		public bool IsRepeated => LineType.SysName == "Repeated";

		public VisualBlockLine()
		{

		}

		public VisualBlockLine(string id, int order)
		{
			BlockLineId = id;
			Order = order;
			Fields = new List<VisualBlockField>();
		}

		public static explicit operator VisualBlockParticipantLine(VisualBlockLine other)
		{
			return new VisualBlockParticipantLine
			{
				Values = other.Fields.Select(x => (VisualBlockParticipantField) x).ToList(),
				BlockLineId = other.BlockLineId,
				Order = other.Order
			};
		}
	}
}
