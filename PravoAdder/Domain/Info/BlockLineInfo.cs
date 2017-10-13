using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PravoAdder.Domain
{
	public class BlockLineInfo : ICloneable
	{
		public string Id { get; set; }

		public string BlockLineId { get; set; }

		[JsonProperty(PropertyName = "Values")]
		public ICollection<BlockFieldInfo> Fields { get; set; } = new List<BlockFieldInfo>();

		[JsonProperty(PropertyName = "Order")]
		public int Order { get; set; }

		public BlockLineInfo CloneWithFields(ICollection<BlockFieldInfo> fields)
		{
			return new BlockLineInfo
			{
				Fields = new List<BlockFieldInfo>(fields),
				BlockLineId = BlockLineId,
				Order = Order
			};
		}

		public object Clone()
		{
			return new BlockLineInfo
			{
				Fields = new List<BlockFieldInfo>(Fields),
				BlockLineId = BlockLineId,
				Order = Order
			};
		}

		public BlockLineInfo()
		{
			
		}

		public BlockLineInfo(string id, int order)
		{
			BlockLineId = id;
			Order = order;
		}

		public BlockLineInfo(string id, int order, ICollection<BlockFieldInfo> fields)
		{
			BlockLineId = id;
			Order = order;
			Fields = new List<BlockFieldInfo>(fields);
		}
	}
}