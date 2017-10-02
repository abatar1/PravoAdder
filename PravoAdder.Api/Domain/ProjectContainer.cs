using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class ProjectContainer
	{
		public ProjectContainer(object data)
		{
			foreach (var project in ((dynamic) data).Projects)
			{
				Projects.Add(new Project(project));
			}
		}

		public List<Project> Projects { get; set; } = new List<Project>();
	}
}