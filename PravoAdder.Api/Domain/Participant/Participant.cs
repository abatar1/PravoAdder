using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
    public class Participant : DatabaseEntityItem
	{
		public string TypeName { get; set; }
        public string TypeId { get; set; }
		public string Inn { get; set; }
		public string CreationDate { get; set; }
		public List<VisualBlockParticipantLine> VisualBlockValueLines { get; set; }
	}
}