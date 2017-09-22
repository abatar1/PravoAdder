using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace PravoAdder.Helpers
{
	public static class JsonHelper
	{
		public static IEnumerable<JToken> GetAllChildrens(this JToken json)
		{
			foreach (var c in json.Children())
			{
				yield return c;
				foreach (var cc in GetAllChildrens(c))
					yield return cc;
			}
		}
	}
}
