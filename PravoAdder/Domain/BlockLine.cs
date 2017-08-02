using System.Collections.Generic;
using Newtonsoft.Json;

namespace PravoAdder.Domain
{
    public class BlockLine
    {
        [JsonProperty(PropertyName = "BlockLineId")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "Values")]
        public IEnumerable<BlockField> Fields { get; set; }
    }
}
