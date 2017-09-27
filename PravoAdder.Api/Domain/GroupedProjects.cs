using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class GroupedProjects : DatabaseEntityItem
	{
		public GroupedProjects(object data)
		{
			Projects = new ProjectContainer(data).Projects;
			ProjectGroupResponse = new ProjectGroup((data as dynamic)?.ProjectGroupResponse);
		}

		public List<Project> Projects { get; set; }
		public ProjectGroup ProjectGroupResponse { get; set; }
	}
}
