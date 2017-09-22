using System;
using Newtonsoft.Json;
using PravoAdder.Api.Domain;

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

		public static BlockFieldInfo Create(VisualBlockField field, int index)
		{
			var blockfieldInfo = new BlockFieldInfo
			{
				Id = field.Id,
				Name = field.ProjectField.Name,
				ColumnNumber = index
			};
			var projectField = field.ProjectField;
			var fieldType = projectField.ProjectFieldFormat.SysName;
			switch (fieldType)
			{
				case "Dictionary":
					blockfieldInfo.Type = projectField.ProjectFieldFormat.SysName;
					blockfieldInfo.SpecialData = projectField.ProjectFieldFormat.Dictionary.SystemName;
					break;
				case "Text":
				case "Date":
				case "TextArea":
					blockfieldInfo.Type = "Text";
					break;
				case "Number":
					blockfieldInfo.Type = "Value";
					break;
				case "Participant":
					blockfieldInfo.Type = fieldType;
					break;
				default:
					throw new ArgumentException("Field type doesn't supported.");
			}

			return blockfieldInfo;
		}
	}
}