using System;
using System.Collections.Generic;
using Newtonsoft.Json;
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

		public static T CloneJson<T>(this T source)
		{
			if (ReferenceEquals(source, null))
			{
				return default(T);
			}

			var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };

			return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
		}
	}
}
