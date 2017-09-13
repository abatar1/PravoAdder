namespace PravoAdder.Domain
{
	public class Project : DatabaseEntityItem
	{
		public Project(string name, string id) : base(name, id)
		{
		}

		public Project(object data) : base(data)
		{
		}

		public Project()
		{
		}
	}
}
