using System.Collections.Generic;
using System.Linq;
using PravoAdder.Api.Domain;

namespace PravoAdder.Api.Helpers
{
	public static class DatabaseEntityItemHelper
	{
		public static T GetByName<T>(this IList<T> container, string name) where T : DatabaseEntityItem
		{
			return container?.FirstOrDefault(i => i.Name == name);
		}
	}
}
