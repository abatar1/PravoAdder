namespace PravoAdder.Domain
{
	public class ApplicationArguments
	{
		public string ConfigFileName { get; set; }
		public string SourceFileName { get; set; }
		public ProcessType ProcessType { get; set; }
		public string Password { get; set; }
		public ReaderMode ReaderMode { get; set; }
		public int RowNum { get; set; }
		public int ParallelOptions { get; set; }
		public bool IsOverwrite { get; set; }
		public string ParticipantType { get; set; }
		public string Date { get; set; }
		public string SearchKey { get; set; }
		public string ProjectType { get; set; }
	}
}
