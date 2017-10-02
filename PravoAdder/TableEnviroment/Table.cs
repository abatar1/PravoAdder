using System.Collections.Generic;
using System.Linq;

namespace PravoAdder.TableEnviroment
{
    public class Table
    {
        private readonly Dictionary<FieldInfo, List<int>> _headerRowContentSti;

        public Table(List<Row> table, Row header)
        {
	        Header = header;
            _headerRowContentSti = Header.Content
                .GroupBy(i => i.Value)
                .ToDictionary(g => g.Key, g => g.Select(p => p.Key).ToList());
            TableContent = table;
        }

	    public Row Header { get; }
		public List<Row> TableContent { get; }
		public string Name { get; set; }

        public bool IsComplexRepeat(FieldInfo fieldAddress)
        {
            _headerRowContentSti.TryGetValue(fieldAddress, out List<int> result);
            if (result == null) return false;

            return result
                .Select(index => Header[index])
                .Any(address => address.IsRepeatField && address.RepeatFieldNumber > 0);
        }

	    public FieldInfo GetFullAddressInfo(string blockName, string fieldName)
	    {
			var fieldAddress = new FieldInfo(blockName, fieldName);
		    return _headerRowContentSti.Keys
			    .FirstOrDefault(f => f.Equals(fieldAddress));
	    }

        public Dictionary<int, int> GetComplexIndexes(FieldInfo fieldAddress, int blockNumber = 0)
        {
			return Header
                .Where(x => x.Value.Equals(fieldAddress))
				.Where(i => i.Value.RepeatBlockNumber == blockNumber)
				.ToDictionary(x => x.Value.RepeatFieldNumber, x => x.Key);
        }

	    public List<int> GetRepeatBlockNumber(string blockName)
	    {
		    if (Header.Values.Any(x => x.BlockName == blockName))
		    {
				return Header.Values
					.GroupBy(x => x.BlockName)
					.First(x => x.Key == blockName)
					.Select(x => x.RepeatBlockNumber)
					.Distinct()
					.ToList();
			}
			return new List<int>{0};	    
	    }

        public List<int> GetIndexes(FieldInfo fieldAddress, int blockNumber = 0)
        {
            _headerRowContentSti.TryGetValue(fieldAddress, out List<int> result);
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

        public int TryGetIndex(FieldInfo fieldAddress)
        {
            _headerRowContentSti.TryGetValue(fieldAddress, out List<int> result);
            if (result != null && result.Count == 1) return result.First();
            return 0;
        }

        public bool Contains(FieldInfo fieldAddress)
        {
            return _headerRowContentSti.ContainsKey(fieldAddress);
        }
    }
}