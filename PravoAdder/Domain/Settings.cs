﻿using System.ComponentModel;
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
    }
}
