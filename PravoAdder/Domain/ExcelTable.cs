using System.Collections.Generic;
using System.Linq;

namespace PravoAdder.Domain
{
    public class ExcelTable
    {
        private readonly IDictionary<int, FieldAddress> _info;

        private readonly Dictionary<FieldAddress, List<int>> _infoRowContentSti;

        public ExcelTable(IList<IDictionary<int, string>> table, IDictionary<int, string> info)
        {
            _info = info
                .ToDictionary(i => i.Key, i => new FieldAddress(i.Value));
            _infoRowContentSti = _info
                .GroupBy(i => i.Value)
                .ToDictionary(g => g.Key, g => g.Select(pp => pp.Key).ToList());
            TableContent = table;
        }

        public IList<IDictionary<int, string>> TableContent { get; }

        public bool IsComplexRepeat(FieldAddress fieldAddress)
        {
            _infoRowContentSti.TryGetValue(fieldAddress, out List<int> result);
            if (result == null) return false;

            return result
                .Select(index => _info[index])
                .Any(address => address.Repeat && address.RepeatNumber > 0);
        }

        public Dictionary<int, int> GetComplexIndexes(FieldAddress fieldAddress)
        {
            return _info
                .Where(x => x.Value.Equals(fieldAddress))
                .ToDictionary(x => x.Value.RepeatNumber, x => x.Key);
        }

        public List<int> GetIndexes(FieldAddress fieldAddress)
        {
            _infoRowContentSti.TryGetValue(fieldAddress, out List<int> result);
            if (result == null) return null;

            foreach (var index in result)
            {
                var address = _info[index];
                if (!address.Repeat) return new List<int> {index};

                return _info
                    .Where(i => i.Value.RepeatNumber == address.RepeatNumber && i.Value.Equals(fieldAddress))
                    .Select(i => i.Key)
                    .ToList();
            }
            return null;
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