namespace PravoAdder.Domain
{
	public class ProcessType
	{
		public string Name { get; set; }
		public bool NeedTable { get; set; }

		public ProcessType(string name, bool needTable)
		{
			Name = name;
			NeedTable = needTable;
		}
	}
}
