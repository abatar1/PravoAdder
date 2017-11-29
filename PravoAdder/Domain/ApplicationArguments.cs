namespace PravoAdder.Domain
{
	public class ApplicationArguments
	{
		public string BaseUri { get; set; }
		public string UserName { get; set; }
		public string ConfigFileName { get; set; }
		public string SourceName { get; set; }
		public string SecondSourceName { get; set; }
		public string ProcessType { get; set; }
		public string Password { get; set; }
		public ReadingMode ReaderMode { get; set; }
		public int RowNum { get; set; }
		public int ParallelOptions { get; set; }
		public bool IsOverwrite { get; set; }
		public string ParticipantType { get; set; }
		public string Date { get; set; }
		public string SearchKey { get; set; }
		public string ProjectType { get; set; }
		public string Language { get; set; }
	}
}
