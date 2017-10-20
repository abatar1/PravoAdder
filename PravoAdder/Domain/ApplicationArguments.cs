namespace PravoAdder.Domain
{
	public class ApplicationArguments
	{
		public string ConfigFilename { get; set; }
		public ProcessType ProcessType { get; set; }
		public ReaderMode ReaderMode { get; set; }
		public int RowNum { get; set; }
		public int ParallelOptions { get; set; }
		public bool IsOverwrite { get; set; }
		public string ParticipantType { get; set; }
		public string Date { get; set; }
	}
}
