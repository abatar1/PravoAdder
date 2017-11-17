using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class BillStatusGroup : DatabaseEntityItem
	{
		public List<string> BillIds { get; set; }
		public string BillStatusSysName { get; set; }
	}
}
