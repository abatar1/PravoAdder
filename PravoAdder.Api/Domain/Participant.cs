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
	        VAT = vat;
        }

		public Participant(object data) : base(data)
		{
			var dynamicData = data as dynamic;
			TypeId = dynamicData.TypeId.ToString();
			TypeName = dynamicData.TypeName.ToString();
		}

		public string TypeName { get; private set; }
        public string TypeId { get; private set; }
		public string VAT { get; private set; }

        public Participant ChangeName(string newName)
        {
            return new Participant(newName, Id, TypeName, TypeId, VAT);
        }

        public static Participant TryParse(dynamic participant)
        {
            return new Participant
            {
                Id = participant.Id.ToString(),
                Name = participant.Name.ToString(),
                TypeId = participant.TypeId.ToString(),
                TypeName = participant.TypeName.ToString(),
				VAT = participant.INN.ToString()
            };
        }
    }
}