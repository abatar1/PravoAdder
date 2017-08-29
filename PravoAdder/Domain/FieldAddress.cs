using System;
using System.Text.RegularExpressions;

namespace PravoAdder.Domain
{
	public class FieldAddress : IEquatable<FieldAddress>
	{
		public string FieldName { get; }
		public string BlockName { get; }
		public bool Repeat { get; }
		public int RepeatNumber { get; } = -1;

		public string FullName => $"{BlockName} {FieldName}";

		public FieldAddress(string address)
		{
			var matches = new Regex("\".*?\"").Matches(address);		
			BlockName = FormatBlockName(FormatMatch(matches, 0));
			FieldName = FormatMatch(matches, 1);
			if (address.Contains("Повтор"))
			{
				Repeat = true;
				RepeatNumber = int.Parse(FormatMatch(matches, 2));
			}				
		}

		private static string FormatBlockName(string name)
		{
			return name.Split('-')[0].Trim();
		}

		public override string ToString()
		{
			return FullName;
		}

		public static FieldAddress Create(string address)
		{
			return new FieldAddress(address);
		}

		private static string FormatMatch(MatchCollection matches, int i)
		{
			return matches[i].Value.Replace("\"", "");
		}

		public FieldAddress(string blockName, string fieldName)
		{
			BlockName = FormatBlockName(blockName);
			FieldName = fieldName;
		}

		public bool Equals(FieldAddress other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(FieldName, other.FieldName) && string.Equals(BlockName, other.BlockName);
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
				return ((FieldName != null ? FieldName.GetHashCode() : 0) * 397) ^ (BlockName != null ? BlockName.GetHashCode() : 0);
			}
		}
	}
}
