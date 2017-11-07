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
		public ProjectFolder ProjectFolder { get; set; }
		public string Description { get; set; }
		public static ProjectGroup Empty => new ProjectGroup(null, null);
	}
}
