using System.Collections;
using System.Collections.Generic;

namespace PravoAdder.TableEnviroment
{
	public class Row : IEnumerable<KeyValuePair<int, FieldInfo>>
	{
		public Row(IDictionary<int, FieldInfo> content, IList<string> participants = null)
		{
			Content = content;
			Participants = participants;
		}

		public IDictionary<int, FieldInfo> Content { get; }
		public IList<string> Participants { get; }
		public ICollection<FieldInfo> Values => Content.Values;

		public FieldInfo this[int index] => Content[index];

		public IEnumerator<KeyValuePair<int, FieldInfo>> GetEnumerator()
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
