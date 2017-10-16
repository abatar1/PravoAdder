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
		}

		public string TypeName { get; private set; }
        public string TypeId { get; private set; }
		public string Inn { get; private set; }

        public Participant ChangeName(string newName)
        {
            return new Participant(newName, Id, TypeName, TypeId, Inn);
        }

        public static Participant TryParse(dynamic participant)
        {
            return new Participant
            {
                Id = participant.Id.ToString(),
                Name = participant.Name.ToString(),
                TypeId = participant.TypeId.ToString(),
                TypeName = participant.TypeName.ToString(),
				Inn = participant.INN.ToString()
            };
        }
    }
}