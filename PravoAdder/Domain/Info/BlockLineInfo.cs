using System.Collections.Generic;
using Newtonsoft.Json;

namespace PravoAdder.Domain.Info
{
    public class BlockLineInfo
    {
        [JsonProperty(PropertyName = "BlockLineId")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "Values")]
        public IEnumerable<BlockFieldInfo> Fields { get; set; }

        [JsonProperty(PropertyName = "Order")]
        public int Order { get; set; }
    }
}
