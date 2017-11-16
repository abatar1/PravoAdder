using System;

namespace PravoAdder.Api.Domain
{
	public class GroupItem : DatabaseEntityItem
	{
		public string EntityName { get; set; }
		public string EntityId { get; set; }
		public DateTime Date { get; set; }
	}
}
