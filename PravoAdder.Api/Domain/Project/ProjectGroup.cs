namespace PravoAdder.Api.Domain
{
	public class ProjectGroup : DatabaseEntityItem
	{
		public ProjectGroup()
		{
			
		}

		public ProjectGroup(string name, string id)
		{
			Name = name;
			Id = id;
		}

		public static ProjectGroup Empty => new ProjectGroup(null, null);
	}
}
