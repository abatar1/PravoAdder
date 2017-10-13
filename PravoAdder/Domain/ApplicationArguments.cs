namespace PravoAdder.Domain
{
	public class ApplicationArguments
	{
		public string ConfigFilename { get; set; }
		public ProcessType ProcessType { get; set; }
		public ReaderMode ReaderMode { get; set; }
		public int RowNum { get; set; }
		public int MaxDegreeOfParallelism { get; set; }
	}
}
