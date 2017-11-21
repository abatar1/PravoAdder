namespace PravoAdder.Api.Domain
{
	public class Calendar : DatabaseEntityItem
	{
		public bool IsSmart { get; set; }
		public CalendarColor CalendarColor { get; set; }
	}
}
