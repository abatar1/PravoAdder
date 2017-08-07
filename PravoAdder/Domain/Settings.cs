using Newtonsoft.Json;

namespace PravoAdder.Domain
{
    public class Settings
    {
        public string Login { get; set; }

        [JsonIgnore]
        public string Password { get; set; }

        public string FolderName { get; set; }

        public string ProjectTypeName { get; set; }

        public int DataRowPosition { get; set; }

        public int InformationRowPosition { get; set; }

        public string IdComparerPath { get; set; }
    }
}
