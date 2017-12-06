namespace PravoAdder.Api.Domain
{
	public class DocumentFolder : DatabaseEntityItem
	{
		public Responsible Author { get; set; }
		public string ParentId { get; set; }
		public string ProjectId { get; set; }
	}
}
