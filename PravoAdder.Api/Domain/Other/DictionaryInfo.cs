using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class DictionaryInfo : DatabaseEntityItem
	{
		public string SystemName { get; set; }
		public List<DictionaryItem> Items { get; set; }

		public override string ToString()
		{
			return DisplayName;
		}
	}
}