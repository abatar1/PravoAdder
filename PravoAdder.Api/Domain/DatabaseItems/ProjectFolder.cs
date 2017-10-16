namespace PravoAdder.Api.Domain
{
	public class ProjectFolder : DatabaseEntityItem
	{
		public ProjectFolder(string name, string id) : base(name, id)
		{
		}

		public ProjectFolder(object data) : base(data)
		{
		}

		public ProjectFolder()
		{
		}
	}
}
