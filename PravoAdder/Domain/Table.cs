using System.Collections.Generic;
using System.Linq;

namespace PravoAdder.Domain
{
    public class Table
    {
        private readonly IDictionary<int, FieldAddress> _info;

        private readonly Dictionary<FieldAddress, List<int>> _infoRowContentSti;

        public Table(IList<IDictionary<int, string>> table, IDictionary<int, string> info)
        {
            _info = info
                .ToDictionary(i => i.Key, i => new FieldAddress(i.Value));
            _infoRowContentSti = _info
                .GroupBy(i => i.Value)
                .ToDictionary(g => g.Key, g => g.Select(p => p.Key).ToList());
            TableContent = table;
        }

        public IList<IDictionary<int, string>> TableContent { get; }

        public bool IsComplexRepeat(FieldAddress fieldAddress)
        {
            _infoRowContentSti.TryGetValue(fieldAddress, out List<int> result);
            if (result == null) return false;

            return result
                .Select(index => _info[index])
                .Any(address => address.IsRepeatField && address.RepeatFieldNumber > 0);
        }

	    public bool IsReferenceField(FieldAddress fieldAddress)
	    {
			var isReference = _infoRowContentSti.Keys.FirstOrDefault(f => f.Equals(fieldAddress))?.IsReference;
		    return isReference.HasValue && isReference.Value;
	    }

        public Dictionary<int, int> GetComplexIndexes(FieldAddress fieldAddress, int blockNumber = 0)
        {
			return _info
                .Where(x => x.Value.Equals(fieldAddress))
				.Where(i => i.Value.RepeatBlockNumber == blockNumber)
				.ToDictionary(x => x.Value.RepeatFieldNumber, x => x.Key);
        }

	    public List<int> GetRepeatBlockNumber(string blockName)
	    {
		    return _info.Values
				.GroupBy(x => x.BlockName)
				.First(x => x.Key == blockName)
				.Select(x => x.RepeatBlockNumber)
			    .Distinct()
				.ToList();
	    }

        public List<int> GetIndexes(FieldAddress fieldAddress, int blockNumber = 0)
        {
            _infoRowContentSti.TryGetValue(fieldAddress, out List<int> result);
            if (result == null) return null;

			var resultList = new List<int>();
            foreach (var index in result)
            {
                var address = _info[index];

	            if (!address.IsRepeatBlock)
	            {
		            if (!address.IsRepeatField)
		            {
			            return new List<int> { index };
		            }
					resultList.AddRange(_info
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

					resultList.AddRange(_info
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