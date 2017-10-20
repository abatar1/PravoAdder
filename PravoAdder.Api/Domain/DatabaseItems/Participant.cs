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
		}

		public string TypeName { get; }
        public string TypeId { get; }
		public string Inn { get; }
		public string CreationDate { get; }
    }
}