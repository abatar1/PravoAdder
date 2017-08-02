using System.Collections.Generic;

namespace PravoAdder.Domain
{
    public class Block
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public IEnumerable<BlockLine> Lines { get; set; }
    }
}
