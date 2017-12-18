using System;

namespace PravoAdder.Api.Domain
{
	public class Project : DatabaseEntityItem
	{
		public DateTime CreationDate { get; set; }
		public bool IsArchive { get; set; }
		public Responsible Responsible { get; set; }
		public ProjectType ProjectType { get; set; }
		public ProjectFolder ProjectFolder { get; set; }
		public ProjectGroup ProjectGroup { get; set; }
		public string CasebookNumber { get; set; }
		public string Description { get; set; }
		public Participant Client { get; set; }
		public string DocumentFolderId { get; set; }
		public int? Number { get; set; }
	}
}
