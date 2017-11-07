using Newtonsoft.Json;

namespace PravoAdder.Api.Domain
{
	public class VisualBlockField : DatabaseEntityItem
	{
		public ProjectField ProjectField { get; set; }
		public int Width { get; set; }
		public string Tag { get; set; }
		public bool IsRequired { get; set; }

		[JsonIgnore]
		public int ColumnNumber { get; set; }

		[JsonIgnore]
		public object Value { get; set; }

		public VisualBlockField CloneWithValue(object value)
		{
			return new VisualBlockField
			{
				ColumnNumber = ColumnNumber,
				Id = Id,
				Name = Name,
				Value = value
			};
		}
	}
}
