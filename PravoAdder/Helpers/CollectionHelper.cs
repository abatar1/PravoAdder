using System.Collections.Generic;

namespace PravoAdder.Helpers
{
	public static class CollectionHelper
	{
		public static void AddRange<T1, T2>(this Dictionary<T1, T2> source, Dictionary<T1, T2> collection)
		{
			if (collection == null) return;

			foreach (var item in collection)
			{
				if (!source.ContainsKey(item.Key))
				{
					source.Add(item.Key, item.Value);
				}
			}
		}

		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
		{
			return new HashSet<T>(source);
		}		
	}
}
