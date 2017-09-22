using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace PravoAdder.Domain
{
    public class Settings
    {
        public string Login { get; set; }

        [JsonIgnore]
        public string Password { get; set; }

        [DisplayName("Base uri")]
        public string BaseUri { get; set; }

        [DisplayName("Line number from which the data begins")]
        public int DataRowPosition { get; set; }

        [DisplayName("Line number with information")]
        public int InformationRowPosition { get; set; }

        [DisplayName("Path to the Id file with the table")]
        public string IdComparerPath { get; set; }

	    [DisplayName("Enter block loading mode")]
	    public string BlockLoadingMode { get; set; }

		[DisplayName("Path to xml mapping file")]
		public string XmlMappingPath { get; set; }

        [DisplayName("Overwrite project and project's folders?"), JsonIgnore]
        public bool Overwrite { get; set; }

        [DisplayName("Write source filename"), JsonIgnore]
        public string SourceFileName { get; set; }       

        [DisplayName("Enter list allowed column's colors separated by commas (format:FFRRGGBB)")]
        public string[] AllowedColors { get; set; }

        [DisplayName("Number of threads"), JsonIgnore]
        public int MaxDegreeOfParallelism { get; set; }

        [DisplayName("Number of starting row"), JsonIgnore]
        public int StartRow { get; set; }

        [Ignore(true), JsonIgnore]
        public Dictionary<string, dynamic> AdditionalSettings { get; set; }
    }
}