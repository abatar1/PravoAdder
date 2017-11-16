using System;

namespace PravoAdder.Api.Domain
{
	public class TimeLog : DatabaseEntityItem
	{
		public EventType LogType { get; set; }
		public DateTime LogDate { get; set; }
		public int Time { get; set; }
		public ActivityTag Tag { get; set; }
		public override bool ShouldSerializeId() => false;
	}
}
