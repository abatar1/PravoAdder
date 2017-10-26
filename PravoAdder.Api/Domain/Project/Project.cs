using System;

namespace PravoAdder.Api.Domain
{
	public class Project : DatabaseEntityItem
	{
		public DateTime CreationDate { get; set; }
		public string ProjectTypeId { get; set; }
		public string ProjectGroupId { get; set; }
		public bool IsArchive { get; set; }
		public Responsible Responsible { get; set; }
		public ProjectType ProjectType { get; set; }
	}
}
