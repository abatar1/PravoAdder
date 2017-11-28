using System.Collections.Generic;
using Newtonsoft.Json;

namespace PravoAdder.Api.Domain
{
	public class VisualBlockParticipant : DatabaseEntityItem
	{
		public string VisualBlockId { get; set; }
		public string ProjectId { get; set; }
		public bool IsRepeatable { get; set; }
		public string NameInConstructor { get; set; }
		public int Order { get; set; }
		public int FrontOrder { get; set; }
		public List<VisualBlockParticipantLine> Lines { get; set; }

		[JsonIgnore]
		public string Message { get; set; }
	}
}
