using System;
using System.Text.RegularExpressions;

namespace PravoAdder.Domain
{
	public class FieldAddress : IEquatable<FieldAddress>
	{
		public string FieldName { get; }
		public string BlockName { get; }
		public bool Repeat { get; }

		public FieldAddress(string address)
		{
			var matches = new Regex("\".*?\"").Matches(address);		
			BlockName = FormatMatch(matches, 0);
			FieldName = FormatMatch(matches, 1);
			if (address.Contains("Повтор"))
				Repeat = true;
		}

		public static FieldAddress Create(string address)
		{
			return new FieldAddress(address);
		}

		private string FormatMatch(MatchCollection matches, int i)
		{
			return matches[i].Value.Replace("\"", "");
		}

		public FieldAddress(string blockName, string fieldName)
		{
			BlockName = blockName;
			FieldName = fieldName;
		}

		public bool Equals(FieldAddress other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(FieldName, other.FieldName) && string.Equals(BlockName, other.BlockName) && Repeat == other.Repeat;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((FieldAddress) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (FieldName != null ? FieldName.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (BlockName != null ? BlockName.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ Repeat.GetHashCode();
				return hashCode;
			}
		}
	}
}
