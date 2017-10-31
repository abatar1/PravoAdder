namespace PravoAdder.Api.Domain
{
	public class VisualBlockField
	{
		public string Id { get; set; }
		public ProjectField ProjectField { get; set; }
		public int Width { get; set; }
		public string Tag { get; set; }
		public bool IsRequired { get; set; }

		public override string ToString() => ProjectField.Name;
	}
}
