﻿using System.Collections.Generic;
using System.Linq;

namespace PravoAdder.Domain
{
    public class Table
    {
        private readonly IDictionary<FieldAddress, List<int>> _infoRowContentSti;

        public Table(IEnumerable<Row> table, Row header)
        {
			Header = header;
			_infoRowContentSti = Header
				.GroupBy(i => i.Value)
                .ToDictionary(g => g.Key, g => g.Select(p => p.Key).ToList());
			TableContent = table.ToList();
		}

		public Row Header { get; }
	    public List<Row> TableContent { get; }
	    public string Name { get; set; }

	    public long Size => TableContent.Count;

	    public string GetValue(Row tableRow, string fieldName)
	    {
			var index = Header.Content.FirstOrDefault(h => h.Value.FieldName == fieldName).Key;
		    return tableRow[index].Value?.Trim();
		}

	    public bool TryGetValue(Row tableRow, string fieldName, out string value)
	    {
		    var index = Header.Content.FirstOrDefault(h => h.Value.FieldName == fieldName).Key;
		    if (tableRow.ContainsKey(index))
		    {
			    value = tableRow[index].Value?.Trim();
			    return true;
		    }
		    value = string.Empty;
		    return false;
	    }

		public static string GetValue(Row header, Row tableRow, FieldAddress fieldAddress)
	    {
		    var index = header.Content.First(h => h.Value.FieldName == fieldAddress.FieldName &&
		                                                   h.Value.BlockName == fieldAddress.BlockName).Key;
		    return tableRow[index].Value?.Trim();
	    }   

		public bool IsComplexRepeat(FieldAddress fieldAddress)
        {
            _infoRowContentSti.TryGetValue(fieldAddress, out List<int> result);
            if (result == null) return false;

            return result
                .Select(index => Header[index])
                .Any(address => address.IsRepeatField && address.RepeatFieldNumber > 0);
        }

	    public bool IsReferenceField(FieldAddress fieldAddress)
	    {
			var isReference = _infoRowContentSti.Keys.FirstOrDefault(f => f.Equals(fieldAddress))?.IsReference;
		    return isReference.HasValue && isReference.Value;
	    }

        public Dictionary<int, int> GetComplexIndexes(FieldAddress fieldAddress, int blockNumber = 0)
        {
			return Header
				.Where(x => x.Value.Equals(fieldAddress))
				.Where(i => i.Value.RepeatBlockNumber == blockNumber)
				.ToDictionary(x => x.Value.RepeatFieldNumber, x => x.Key);
        }

	    public List<int> GetRepeatBlockNumber(string blockName)
	    {
		    return Header.Values
				.GroupBy(x => x.BlockName)
				.FirstOrDefault(x => x.Key == blockName)
				?.Select(x => x.RepeatBlockNumber)
			    .Distinct()
				.ToList() ?? new List<int> {0};
	    }

        public List<int> GetIndexes(FieldAddress fieldAddress, int blockNumber = 0)
        {
            _infoRowContentSti.TryGetValue(fieldAddress, out var result);
            if (result == null) return null;

			var resultList = new List<int>();
            foreach (var index in result)
            {
                var address = Header[index];

	            if (!address.IsRepeatBlock)
	            {
		            if (!address.IsRepeatField)
		            {
			            return new List<int> { index };
		            }
					resultList.AddRange(Header
						.Where(i => i.Value.RepeatFieldNumber == address.RepeatFieldNumber && i.Value.Equals(fieldAddress))
			            .Select(i => i.Key));
				}
	            else
	            {
		            if (!address.IsRepeatField)
		            {
			            if (address.RepeatBlockNumber != blockNumber) continue;
			            return new List<int> { index };
					}

					resultList.AddRange(Header
						.Where(i => i.Value.RepeatFieldNumber == address.RepeatFieldNumber && i.Value.Equals(fieldAddress))
						.Where(i => i.Value.RepeatBlockNumber == blockNumber)
				        .Select(i => i.Key));
				}
                
            }
            return resultList;
        }

		public int TryGetIndex(FieldAddress fieldAddress)
        {
            _infoRowContentSti.TryGetValue(fieldAddress, out List<int> result);
            if (result != null && result.Count == 1) return result.First();
            return 0;
        }

        public bool Contains(FieldAddress fieldAddress)
        {
            return _infoRowContentSti.ContainsKey(fieldAddress);
        }	    
    }
}