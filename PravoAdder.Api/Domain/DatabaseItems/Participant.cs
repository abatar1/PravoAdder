using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace PravoAdder.Api.Domain
{
    public class Participant : DatabaseEntityItem
	{
        public Participant()
        {
        }

		public Participant(string name, string id, string typeName, string typeId, string vat) : base(name, id)
        {
            TypeName = typeName;
            TypeId = typeId;
	        Inn = vat;
        }

		public Participant(object data) : base(data)
		{
			var dynamicData = data as dynamic;

			TypeId = dynamicData?.TypeId?.ToString() ?? dynamicData?.Type?.Id;
			TypeName = dynamicData?.TypeName?.ToString() ?? dynamicData?.Type?.Name;
			Inn = dynamicData?.INN?.ToString();
			CreationDate = dynamicData?.CreationDate?.ToString();

			var jLines = dynamicData?.VisualBlockValueLines;
			if (jLines != null)
			{
				VisualBlockValueLines = ((JArray) jLines).ToObject<List<VisualBlockParticipantLine>>();
			}
		}

		public string TypeName { get; set; }
        public string TypeId { get; set; }
		public string Inn { get; set; }
		public string CreationDate { get; set; }
		public List<VisualBlockParticipantLine> VisualBlockValueLines { get; set; }
	}
}