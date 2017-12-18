namespace PravoAdder.Domain
{
	public class Instance
	{
		public string Name { get; set; }
		public string FileName { get; set; }

		public bool IsEmpty => FileName == null || Name == null;
	}
}
