using System.Collections.Generic;
using System.Linq;
using PravoAdder.Domain;

namespace PravoAdder.Helpers
{
	public static class RowHelper
	{
		public static Row Concat(this Row row1, Row row2)
		{
			var concatedDict = row1.Content.Concat(row2.Content)
				.ToDictionary(x => x.Key, x => x.Value);
			return new Row(concatedDict);
		}

		public static Dictionary<int, FieldAddress> ConcatFromDictionary(this Dictionary<int, FieldAddress> row1, Dictionary<int, FieldAddress> row2)
		{
			return row1.Concat(row2)
				.ToDictionary(x => x.Key, x => x.Value);
		}
	}
}
