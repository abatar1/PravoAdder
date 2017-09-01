namespace PravoAdder.Domain
{
	public class Participant
	{
		public string Name { get; private set; }
		public string Id { get; private set; }
		public string TypeName { get; private set; }
		public string TypeId { get; private set; }

		public Participant()
		{
		}

		public Participant(string name, string id, string typeName, string typeId)
		{
			Name = name;
			Id = id;
			TypeName = typeName;
			TypeId = typeId;
		}

		public static Participant TryParse(dynamic participant)
		{
			return new Participant
			{
				Id = participant.Id.ToString(),
				Name = participant.Name.ToString(),
				TypeId = participant.TypeId.ToString(),
				TypeName = participant.TypeName.ToString()
			};
		}
	}
}
