using System.Collections.Generic;
using System.Linq;
using PravoAdder.Domain;

namespace PravoAdder.Helpers
{
	public static class DatabaseEntityItemHelper
	{
		public static T GetByName<T>(this IList<T> container, string name) where T : DatabaseEntityItem
		{
			return container
				.FirstOrDefault(i => i.Name == name);
		}
	}
}
