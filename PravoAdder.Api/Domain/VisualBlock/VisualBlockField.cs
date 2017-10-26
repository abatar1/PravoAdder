namespace PravoAdder.Api.Domain
{
	public class VisualBlockField
	{
		public string Id { get; set; }
		public ProjectField ProjectField { get; set; }

		public override string ToString() => ProjectField.Name;
	}
}
