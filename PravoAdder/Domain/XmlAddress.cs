using System;

namespace PravoAdder.Domain
{
	public class XmlAddress : IEquatable<XmlAddress>
	{
		public XmlAddress(string blockName, string fieldName, int count,
			bool repeatBlock = false, int repeatBlockNumber = 0)
		{
			BlockName = blockName;
			FieldName = fieldName;
			Count = count;
			IsRepeatBlock = repeatBlock;
			RepeatBlockNumber = repeatBlockNumber;
			if (count > MaxCount) MaxCount = count;
		}

		public string BlockName { get; }
		public string FieldName { get; }
		public int Count { get; }

		public bool IsRepeatBlock { get; }
		public int RepeatBlockNumber { get; }
		
		public static int MaxCount = 1;

		public override string ToString()
		{
			return $"{BlockName} {FieldName} {Count} {RepeatBlockNumber}";
		}

		public FieldAddress ToFieldAddress()
		{
			return new FieldAddress(BlockName, FieldName, IsRepeatBlock, RepeatBlockNumber);
		}

		public XmlAddress ToRepeatBlock(int count, int number)
		{
			return new XmlAddress(BlockName, FieldName, count, true, number);
		}

		public bool NameEquals(XmlAddress other)
		{
			return other.BlockName == BlockName && other.FieldName == FieldName;
		}

		public bool Equals(XmlAddress other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(BlockName, other.BlockName) && string.Equals(FieldName, other.FieldName) && Count == other.Count && RepeatBlockNumber == other.RepeatBlockNumber;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((XmlAddress) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (BlockName != null ? BlockName.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (FieldName != null ? FieldName.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ Count;
				hashCode = (hashCode * 397) ^ RepeatBlockNumber;
				return hashCode;
			}
		}
	}
}
