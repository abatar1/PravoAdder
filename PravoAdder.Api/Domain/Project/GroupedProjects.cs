using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class GroupedProjects
	{
		public List<Project> Projects { get; set; }
		public ProjectGroup ProjectGroupResponse { get; set; }
	}
}
