namespace PravoAdder.Domain
{
	public class ProjectGroup
	{
		public ProjectGroup(string name, string id)
		{
			Name = name;
			Id = id;
		}

		public string Name { get; }
		public string Id { get; }
	}
}
