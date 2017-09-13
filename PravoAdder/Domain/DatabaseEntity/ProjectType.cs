namespace PravoAdder.Domain
{
	public class ProjectType : DatabaseEntityItem
	{
		public ProjectType(string name, string id) : base(name, id)
		{
		}

		public ProjectType(object data) : base(data)
		{
		}

		public ProjectType()
		{
		}
	}
}
