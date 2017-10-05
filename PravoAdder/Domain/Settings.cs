using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace PravoAdder.Domain
{
    public class Settings
	{
		[ProcessType(ProcessType.All)]
		public string Login { get; set; }

        [JsonIgnore, ProcessType(ProcessType.All)]
        public string Password { get; set; }

        [DisplayName("Base uri"), ProcessType(ProcessType.All)]
        public string BaseUri { get; set; }

	    [DisplayName("Enter block loading mode"), ProcessType(ProcessType.Migration)]
	    public ReaderMode BlockReadingMode { get; set; }

		[DisplayName("Enter filename with ignore indexes"), ProcessType(ProcessType.Migration)]
		public string IgnoreFilePath { get; set; }

		[DisplayName("Line number from which the data begins"), ProcessType(ProcessType.Migration), ReadingType(ReaderMode.Excel)]
        public int DataRowPosition { get; set; }

		[DisplayName("Line number with information"), ProcessType(ProcessType.Migration), ReadingType(ReaderMode.Excel)]
        public int InformationRowPosition { get; set; }

		[DisplayName("Path to xml mapping file"), ProcessType(ProcessType.Migration), ReadingType(ReaderMode.XmlMap)]
		public string XmlMappingPath { get; set; }

        [DisplayName("Overwrite project and project's folders?"), JsonIgnore, ProcessType(ProcessType.All)]
        public bool Overwrite { get; set; }

		[DisplayName("Write source filename"), JsonIgnore, ProcessType(ProcessType.Migration)]
        public string SourceFileName { get; set; }       

        [DisplayName("Enter list allowed column's colors separated by commas (format:FFRRGGBB)"), ProcessType(ProcessType.Migration), ReadingType(ReaderMode.Excel)]
        public string[] AllowedColors { get; set; }

        [DisplayName("Number of threads"), JsonIgnore, ProcessType(ProcessType.All)]
        public int MaxDegreeOfParallelism { get; set; }

        [DisplayName("Number of starting row"), JsonIgnore, ProcessType(ProcessType.Migration)]
        public int StartRow { get; set; }

		[DisplayName("Number of maximum rows"), JsonIgnore, ProcessType(ProcessType.Migration)]
		public int MaximumRows { get; set; }

		[DisplayName("Date time"), JsonIgnore, ProcessType(ProcessType.CleanByDate)]
		public string DateTime { get; set; }

        [Ignore(true), JsonIgnore, ProcessType(ProcessType.All)]
        public Dictionary<string, dynamic> AdditionalSettings { get; set; }
    }

}