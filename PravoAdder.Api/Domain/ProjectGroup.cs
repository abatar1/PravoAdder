namespace PravoAdder.Api.Domain
{
	public class ProjectGroup : DatabaseEntityItem
	{
		public ProjectGroup(string name, string id) : base(name, id)
		{
		}

		public ProjectGroup(object data) : base(data)
		{
		}

		public ProjectGroup()
		{
		}
	}
}
