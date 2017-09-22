using System.Collections.Generic;
using System.Linq;

namespace PravoAdder.Domain.Info
{
	public class BlockInfo
	{
		public BlockInfo(string name, string id, IEnumerable<BlockLineInfo> lines)
		{
			Name = name;
			Id = id;
			Lines = lines
				.Select(line => (BlockLineInfo) line.Clone());
		}

		public string Name { get; set; }
		public string Id { get; set; }
		public IEnumerable<BlockLineInfo> Lines { get; set; }
	}
}