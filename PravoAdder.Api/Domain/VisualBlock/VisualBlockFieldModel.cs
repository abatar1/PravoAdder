using Newtonsoft.Json;
using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class VisualBlockFieldModel : DatabaseEntityItem
	{
		public ProjectField ProjectField { get; set; }
		public int Width { get; set; }
		public string Tag { get; set; }
		public bool IsRequired { get; set; }

		[JsonIgnore]
		public int ColumnNumber { get; set; }

		[JsonIgnore]
		public object Value { get; set; }

		public VisualBlockFieldModel CloneWithValue(object value)
		{
			return new VisualBlockFieldModel
			{
				ColumnNumber = ColumnNumber,
				Id = Id,
				Name = Name,
				Value = value
			};
		}

		public bool IsTypeOf(string type) => ProjectField.ProjectFieldFormat.Name == type;

		public override string ToString() => ProjectField.Name;

		public override bool Equals(object obj)
		{
			var field = obj as VisualBlockFieldModel;
			return field != null &&
				   Width == field.Width &&
			       ProjectField.Equals(field.ProjectField);
		}

		public override int GetHashCode()
		{
			var hashCode = 267293898;
			hashCode = hashCode * -1521134295 + Width.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<ProjectField>.Default.GetHashCode(ProjectField);
			return hashCode;
		}

		public static explicit operator VisualBlockField(VisualBlockFieldModel other)
		{
			return new VisualBlockField
			{
				Value = other.Value,
				VisualBlockProjectFieldId = other.Id
			};
		}
	}
}
