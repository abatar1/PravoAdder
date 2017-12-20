using System;
using System.Linq;
using System.Text;
using Fclp;
using PravoAdder.Helpers;

namespace PravoAdder.Domain
{
    public class FieldAddress : IEquatable<FieldAddress>
    {
		public FieldAddress(string address, FieldReadingMode readingMode)
		{
			FieldReadingMode = readingMode;
			switch (readingMode)
			{
				case FieldReadingMode.Param:
					ReadAsParam(address);
					break;
				case FieldReadingMode.Slash:
					ReadAsSlash(address);
					break;
			}
		}

	    private void ReadAsSlash(string address)
	    {
		    var parsedAddress = address
				.Split('/')
				.Select(x => x.Trim())
				.ToArray();
		    if (parsedAddress.Length == 1)
		    {
			    Value = parsedAddress[0];
		    }
		    else
		    {
				BlockName = parsedAddress[0];
			    FieldName = parsedAddress[1];
			}	    
	    }

	    private void ReadAsParam(string address)
	    {
			var parser = new FluentCommandLineParser();

		    parser.Setup<string>('b')
			    .Callback(blockName => BlockName = blockName)
			    .Required();
		    parser.Setup<string>('f')
			    .Callback(fieldName => FieldName = fieldName)
			    .Required();
		    parser.Setup<int>('r')
			    .Callback(repeatFieldnumber =>
			    {
				    RepeatFieldNumber = repeatFieldnumber;
			    })
			    .SetDefault(-1);
		    parser.Setup<int>('m')
			    .Callback(repeatBlockNumber =>
			    {
				    if (repeatBlockNumber != 0) IsRepeatBlock = true;
				    RepeatBlockNumber = repeatBlockNumber;
			    })
			    .SetDefault(0);
		    parser.Setup<string>('s')
			    .Callback(referenceAddress =>
			    {
				    if (!string.IsNullOrEmpty(referenceAddress)) IsReference = true;
				    Reference = referenceAddress;
			    })
			    .SetDefault(string.Empty);
		    parser.Setup<string>('k')
			    .Callback(key =>
			    {
				    if (!string.IsNullOrEmpty(key)) IsKey = true;
			    })
			    .SetDefault(string.Empty);

		    var formatAddress = address.Replace('=', ' ').Replace('\"', ' ').Trim();
		    var result = parser.Parse(formatAddress.GetCommandsFromString());
		    if (result.HasErrors)
		    {
			    Value = address;
		    }
		}

		public FieldAddress(string blockName, string fieldName, bool repeatBlock = false, int repeatBlockNumber = 0)
		{
			BlockName = blockName;
			FieldName = fieldName;
			IsRepeatBlock = repeatBlock;
			RepeatBlockNumber = repeatBlockNumber;
		}

	    public FieldAddress()
	    {
	    }

		private FieldReadingMode FieldReadingMode { get; }

		public string BlockName { get; private set; }

		private string _fieldName;
		public string FieldName
		{
			get => _fieldName;
			private set => _fieldName = value.Split('-')[0].Trim();
		}

		private string _fullName;

	    public string FullName
		{
			get
			{
				if (!string.IsNullOrEmpty(_fullName)) return _fullName;
				var fullName = new StringBuilder();
				switch (FieldReadingMode)
				{
					case FieldReadingMode.Param:
						fullName.Append(Value);
						if (!IsValue)
						{
							fullName.Append($"-b {BlockName} -f {FieldName}");
							if (IsRepeatField) fullName.Append($" -r {RepeatFieldNumber}");
							if (IsRepeatBlock) fullName.Append($" -m {RepeatBlockNumber}");
							if (IsReference) fullName.Append($" -s {Reference}");
						}
						break;
					case FieldReadingMode.Slash:
						fullName.Append(Value);
						if (!IsValue)
						{
							fullName.Append($"{BlockName} / {FieldName}");
						}
						break;
				}
				_fullName = fullName.ToString();
				return _fullName;
			}
		}

	    public bool IsValue => Value != null;
	    public string Value { get; set; }

		public bool IsRepeatField => RepeatFieldNumber != -1;
	    public int RepeatFieldNumber { get; set; } = -1;

		public bool IsRepeatBlock { get; private set; }
		public int RepeatBlockNumber { get; private set; }

		public bool IsReference { get; private set; }
		public string Reference { get; private set; }

		public bool IsKey { get; private set; }

	    public bool IsSystem => HeaderBlockInfo.SystemNames.Select(k => k.Value).Contains(BlockName);	    

		public bool Equals(FieldAddress other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(FieldName, other.FieldName) && string.Equals(BlockName, other.BlockName);
		}

		public override string ToString() => FullName;

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
				return ((FieldName != null ? FieldName.GetHashCode() : 0) * 397) ^
					    (BlockName != null ? BlockName.GetHashCode() : 0);
			}
		}
	}
}