using System.Collections.Generic;
using Newtonsoft.Json;

namespace PravoAdder.Api.Domain
{
	public class VisualBlockLine
	{
		public string Id { get; set; }
		public string BlockLineId { get; set; }
		public List<VisualBlockField> Fields { get; set; }
		public LineType LineType { get; set; }
		public int Order { get; set; }

		[JsonIgnore]
		public bool IsSimple => LineType.SysName == "Simple";

		[JsonIgnore]
		public bool IsRepeated => LineType.SysName == "Repeated";
	}
}
