using System;
using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class Event : GroupItem, ICreatable
	{
		public Project Project { get; set; }
		public EventType EventType { get; set; }
		public string Description { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public List<Responsible> Attendees { get; set; }
		public bool AllDay { get; set; }
		public Calendar Calendar { get; set; }
		public List<string> TimeLogs { get; set; }
	}
}
