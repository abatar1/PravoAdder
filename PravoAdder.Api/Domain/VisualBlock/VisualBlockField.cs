using Newtonsoft.Json;
using System.Collections.Generic;

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

		public override string ToString() => ProjectField.Name;

		public override bool Equals(object obj)
		{
			var field = obj as VisualBlockField;
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

		public static explicit operator VisualBlockParticipantField(VisualBlockField other)
		{
			return new VisualBlockParticipantField
			{
				Value = other.Value,
				VisualBlockProjectFieldId = other.Id
			};
		}
	}
}
