using System.Collections.Generic;
using System.Linq;

namespace PravoAdder.Domain
{
	public class ExcelTable
	{
		public IList<IDictionary<int, string>> TableContent { get; }

		private readonly Dictionary<FieldAddress, int> _infoRowContentSti;

		public ExcelTable(IList<IDictionary<int, string>> table, IDictionary<int, string> info)
		{
			var tmp = info
				.ToDictionary(i => i.Key, i => new FieldAddress(i.Value))
				.GroupBy(i => i.Value.Repeat)
				.ToDictionary(g => g.Key, g => g.Select(pp => new {Value = pp.Value, Position = pp.Key}).ToList());
			TableContent = table;
			_infoRowContentSti = new Dictionary<FieldAddress, int>(info
				.GroupBy(p => p.Value)
				.ToDictionary(g => new FieldAddress(g.Key), g => g.Select(pp => pp.Key).First()));
		}

		public int GetIndex(FieldAddress fieldAddress)
		{
			return !_infoRowContentSti.TryGetValue(fieldAddress, out int result) ? default(int) : result;
		}

		public bool Contains(FieldAddress fieldAddress)
		{
			return _infoRowContentSti.ContainsKey(fieldAddress);
		}
	}
}
