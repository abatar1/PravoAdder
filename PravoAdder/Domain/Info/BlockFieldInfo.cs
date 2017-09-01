using Newtonsoft.Json;

namespace PravoAdder.Domain.Info
{
	public class BlockFieldInfo
	{
		[JsonIgnore]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "VisualBlockProjectFieldId")]
		public string Id { get; set; }

		[JsonIgnore]
		public int ColumnNumber { get; set; }

		[JsonProperty(PropertyName = "Value")]
		public object Value { get; set; }

		[JsonIgnore]
		public string Type { get; set; }

		[JsonIgnore]
		public string SpecialData { get; set; }

		public BlockFieldInfo CloneWithValue(object value)
		{
			return new BlockFieldInfo
			{
				ColumnNumber = ColumnNumber,
				Id = Id,
				Name = Name,
				SpecialData = SpecialData,
				Type = Type,
				Value = value
			};
		}
	}
}