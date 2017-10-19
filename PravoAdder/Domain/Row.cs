using System.Collections;
using System.Collections.Generic;

namespace PravoAdder.Domain
{
	public class Row : IEnumerable<KeyValuePair<int, FieldAddress>>
	{
		public Row(IDictionary<int, FieldAddress> content)
		{
			Content = content;
		}

		public Row(IDictionary<int, FieldAddress> content, KeyValuePair<string, string> participant)
		{
			Content = content;
			Participants = new Dictionary<string, string> {{participant.Key, participant.Value}};
		}

		public IDictionary<int, FieldAddress> Content { get; }
		public ICollection<FieldAddress> Values => Content.Values;

		public Dictionary<string, string> Participants { get; }

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
