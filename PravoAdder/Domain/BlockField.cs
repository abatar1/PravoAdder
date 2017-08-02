using Newtonsoft.Json;

namespace PravoAdder.Domain
{
    public class BlockField
    {
        [JsonIgnore]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "VisualBlockProjectFieldId")]
        public string Id { get; set; }

        [JsonIgnore]
        public int ColumnNumber { get; set; }

        [JsonProperty(PropertyName = "Value")]
        public string Value { get; set; }
    }
}
