using System;

namespace PravoAdder.Api.Domain
{
	[Serializable]
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

		public static ProjectGroup Empty => new ProjectGroup(null, null);
	}
}
