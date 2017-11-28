using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class VisualBlockParticipantLine : DatabaseEntityItem
	{
		public string BlockLineId { get; set; }
		public List<VisualBlockParticipantField> Values { get; set; }
		public int Order { get; set; }
	}
}
