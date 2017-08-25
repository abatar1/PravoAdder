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

        [DisplayName("Folder name")]
        public string FolderName { get; set; }

        [DisplayName("Project type name")]
        public string ProjectTypeName { get; set; }

        [DisplayName("Line number from which the data begins")]
        public int DataRowPosition { get; set; }

        [DisplayName("Line number with information")]
        public int InformationRowPosition { get; set; }

        [DisplayName("Path to the Id file with the table")]
        public string IdComparerPath { get; set; }

	    [JsonIgnore]
	    [DisplayName("Overwrite project and project's folders?")]
		public bool Overwrite { get; set; }

	    [JsonIgnore]
	    [DisplayName("Write excel filename")]
	    public string ExcelFileName { get; set; }

	    [DisplayName("Enter block loading mode")]
	    public string BlockLoadingMode { get; set; }

	    [DisplayName("Enter list allowed column's colors separated by commas (format:FFRRGGBB)")]
		public string[] AllowedColors { get; set; }

	    [JsonIgnore]
		[SettingsIgnore(true)]
		public Dictionary<string, dynamic> AdditionalSettings { get; set; } 
	}
}
