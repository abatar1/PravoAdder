namespace PravoAdder.Api.Domain
{
	public abstract class DatabaseEntityItem
	{
		public DatabaseEntityItem()
		{
			
		}

		public DatabaseEntityItem(string name, string id)
		{
			Name = name;
			Id = id;
		}

		public DatabaseEntityItem(object data)
		{
			var dynamicData = data as dynamic;
			Name = dynamicData?.Name?.ToString();
			Id = dynamicData?.Id?.ToString();
		}

		public string Name { get; protected set; }
		public string Id { get; protected set; }

		public override string ToString()
		{
			return Name;
		}
	}
}
