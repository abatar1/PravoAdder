using System.Collections.Generic;
using Newtonsoft.Json;

namespace PravoAdder.Api.Domain
{
	public class Event : DatabaseEntityItem, ICreatable
	{
		public Project Project { get; set; }
		public EventType EventType { get; set; }
		public string Description { get; set; }
		public string StartDate { get; set; }
		public string EndDate { get; set; }
		public List<Responsible> Attendees { get; set; }
		public bool AllDay { get; set; }
		public Calendar Calendar { get; set; }
		public List<string> TimeLogs { get; set; }
	}
}
