using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class ProjectFieldFormat : DatabaseEntityItem
	{
		public DictionaryInfo Dictionary { get; set; }
		public string ParentId { get; set; }
		public List<ProjectFieldFormat> Childrens { get; set; }
	}
}
