using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PravoAdder.Domain
{
	public class Row : IEnumerable<KeyValuePair<int, FieldAddress>>
	{
		public Row(Dictionary<int, FieldAddress> content)
		{
			Content = content;
		}

		public Row(Dictionary<int, FieldAddress> content, KeyValuePair<string, string> participant)
		{
			Content = content;
			Participants = new Dictionary<string, string> {{participant.Key, participant.Value}};
		}

		public Dictionary<int, FieldAddress> Content { get; }
		public List<FieldAddress> Values => Content.Values.ToList();

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
