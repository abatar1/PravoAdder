using System.Collections;
using System.Collections.Generic;

namespace PravoAdder.Domain
{
	public class Row : IEnumerable<KeyValuePair<int, FieldAddress>>
	{
		public Row(IDictionary<int, FieldAddress> content, IList<string> participants)
		{
			Content = content;
			Participants = participants;
		}

		public Row(IDictionary<int, FieldAddress> content, KeyValuePair<string, string> vat)
		{
			Content = content;
			Vat = vat;
		}

		public Row(IDictionary<int, FieldAddress> content)
		{
			Content = content;
		}

		public IDictionary<int, FieldAddress> Content { get; }
		public ICollection<FieldAddress> Values => Content.Values;

		public IList<string> Participants { get; }
		public KeyValuePair<string, string> Vat { get; }

		public FieldAddress this[int index] => Content[index];

		public IEnumerator<KeyValuePair<int, FieldAddress>> GetEnumerator()
		{
			return Content.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public bool ContainsKey(int key)
		{
			return Content.ContainsKey(key);
		}
	}
}
