namespace PravoAdder.Api.Domain
{
	public class LineType
	{
		public string Name { get; set; }
		public string SysName { get; set; }

		public override string ToString() => Name;
	}
}
