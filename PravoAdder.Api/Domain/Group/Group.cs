using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class Group
	{
		public string Date { get; set; }
		public List<GroupItem> Result { get; set; }
	}
}
