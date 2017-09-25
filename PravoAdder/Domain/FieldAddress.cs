using System;
using System.Collections.Generic;
using System.Text;
using Fclp;

namespace PravoAdder.Domain
{
    public class FieldAddress : IEquatable<FieldAddress>
    {
        public FieldAddress(string address)
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
			        if (repeatFieldnumber != -1) RepeatField = true;
			        RepeatFieldNumber = repeatFieldnumber;
		        })
				.SetDefault(-1);
	        parser.Setup<int>('m')
		        .Callback(repeatBlockNumber =>
		        {
			        if (repeatBlockNumber != 0) RepeatBlock = true;
			        RepeatBlockNumber = repeatBlockNumber;
		        })
		        .SetDefault(0);
	        var result = parser.Parse(GetCommandLineFromLine(address));
			if (result.HasErrors) throw new ArgumentException($"Error while parsing table header at {address}");
        }

	    private static string[] GetCommandLineFromLine(string line)
	    {
		    var preResult = line.Split(' ');
		    var result = new List<string>();

			for (var i = 0; i < preResult.Length; i++)
		    {
			    result.Add(preResult[i]);
				if (preResult[i].StartsWith("-")) continue;

				var count = 1;
				while (true)
			    {				    		   
				    if (i + count >= preResult.Length || preResult[i + count].StartsWith("-"))
				    {
					    i += count - 1;
					    break;
				    }
				    result[result.Count - 1] += $" {preResult[i + count]}";
					count += 1;
				}				
			}

		    return result.ToArray();
	    }

        public FieldAddress(string blockName, string fieldName, bool repeatBlock = false, int repeatBlockNumber = 0)
        {
            BlockName = blockName;
            FieldName = fieldName;
	        RepeatBlock = repeatBlock;
	        RepeatBlockNumber = repeatBlockNumber;
        }

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
			    var fullName = new StringBuilder();
			    fullName.Append($"-b {BlockName} -f {FieldName}");
			    if (RepeatField) fullName.Append($" -r {RepeatFieldNumber}");
			    if (RepeatBlock) fullName.Append($" -m {RepeatBlockNumber}");

			    _fullName = fullName.ToString();
			    return _fullName;
		    }
	    }

		public bool RepeatField { get; private set; }
	    public int RepeatFieldNumber { get; private set; } = -1;
		public bool RepeatBlock { get; private set; }
	    public int RepeatBlockNumber { get; private set; }

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