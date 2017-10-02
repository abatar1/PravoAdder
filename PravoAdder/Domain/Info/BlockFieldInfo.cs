using System;
using Newtonsoft.Json;
using PravoAdder.Api.Domain;
using PravoAdder.Helpers;

namespace PravoAdder.Domain
{
	[Serializable]
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

		[JsonIgnore]
		public bool IsReference { get; set; }

		[JsonIgnore]
		public string Reference { get; set; }

		public BlockFieldInfo CloneWithValue(object value)
		{
			var newFieldObject = this.DeepClone();
			newFieldObject.Value = value;
			return newFieldObject;
		}

		public static BlockFieldInfo Create(VisualBlockField field, int index, string reference = null)
		{			
			var blockfieldInfo = new BlockFieldInfo
			{
				Id = field.Id,
				Name = field.ProjectField.Name,
				ColumnNumber = index
			};
			if (reference != null)
			{
				blockfieldInfo.IsReference = true;
				blockfieldInfo.Reference = reference;
			}

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