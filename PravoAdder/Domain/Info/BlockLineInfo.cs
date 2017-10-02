using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PravoAdder.Helpers;

namespace PravoAdder.Domain
{
	[Serializable]
	public class BlockLineInfo : ICloneable
	{
		[JsonProperty(PropertyName = "BlockLineId")]
		public string Id { get; set; }

		[JsonProperty(PropertyName = "Values")]
		public ICollection<BlockFieldInfo> Fields { get; set; } = new List<BlockFieldInfo>();

		[JsonProperty(PropertyName = "Order")]
		public int Order { get; set; }

		public BlockLineInfo CloneWithFields(ICollection<BlockFieldInfo> fields)
		{
			return new BlockLineInfo
			{
				Fields = new List<BlockFieldInfo>(fields),
				Id = Id,
				Order = Order
			};
		}

		public object Clone()
		{
			return this.DeepClone();
		}

		public BlockLineInfo()
		{
			
		}

		public BlockLineInfo(string id, int order)
		{
			Id = id;
			Order = order;
		}

		public BlockLineInfo(string id, int order, ICollection<BlockFieldInfo> fields)
		{
			Id = id;
			Order = order;
			Fields = new List<BlockFieldInfo>(fields);
		}
	}
}