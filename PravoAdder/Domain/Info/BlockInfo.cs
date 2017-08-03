using System.Collections.Generic;

namespace PravoAdder.Domain.Info
{
    public class BlockInfo
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public IEnumerable<BlockLineInfo> Lines { get; set; }
    }
}
