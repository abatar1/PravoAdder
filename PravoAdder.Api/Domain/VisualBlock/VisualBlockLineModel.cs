using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace PravoAdder.Api.Domain
{
	public class VisualBlockLineModel : DatabaseEntityItem, ICreatable
	{		
		public string BlockLineId { get; set; }
		public List<VisualBlockFieldModel> Fields { get; set; }
		public List<VisualBlockField> Values { get; set; }
		public LineType LineType { get; set; }
		public int Order { get; set; }

		[JsonIgnore]
		public bool IsSimple => LineType.SysName == "Simple";

		[JsonIgnore]
		public bool IsRepeated => LineType.SysName == "Repeated";

		public VisualBlockLineModel()
		{

		}

		public VisualBlockLineModel(string id, int order)
		{
			BlockLineId = id;
			Order = order;
			Fields = new List<VisualBlockFieldModel>();
		}

		public static explicit operator VisualBlockLine(VisualBlockLineModel other)
		{
			return new VisualBlockLine
			{
				Values = other.Fields.Select(x => (VisualBlockField) x).ToList(),
				BlockLineId = other.BlockLineId,
				Order = other.Order
			};
		}
	}
}
