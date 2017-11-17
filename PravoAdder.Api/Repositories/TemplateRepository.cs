using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PravoAdder.Api.Domain;

namespace PravoAdder.Api.Repositories
{
	public abstract class TemplateRepository<T> where T : DatabaseEntityItem
	{
		protected static ConcurrentDictionary<string, T> Container;
		protected static IApi<T> Api;

		public static bool IsContainerEmpty;
		private static readonly object IsContainerEmptyLock = new object();	

		private static void FillContainer<TApi>(HttpAuthenticator authenticator, string optional = null) where TApi : IApi<T>
		{
			lock (IsContainerEmptyLock)
			{
				IsContainerEmpty = Container == null;
			}

			if (IsContainerEmpty)
			{
				Api = (IApi<T>) Activator.CreateInstance(typeof(TApi));

				var container = Api.GetMany(authenticator, optional)
					.Select(x => new KeyValuePair<string, T>(x.Name?.ToLower() ?? x.DisplayName.ToLower(), x))
					.GroupBy(x => x.Key)
					.Select(g => g.First())
					.ToList();
				if (optional != null)
				{
					if (Container == null) Container = new ConcurrentDictionary<string, T>();
					container.ForEach(c => Container.AddOrUpdate(c.Key, c.Value, (s, arg2) => arg2));
				}
				else
				{
					Container = new ConcurrentDictionary<string, T>(container);
				}				
			}
		}

		public static List<T> GetMany<TApi>(HttpAuthenticator authenticator, string optional = null) where TApi : IApi<T>
		{
			FillContainer<TApi>(authenticator, optional);
			return Container?.Values.ToList() ?? new List<T>();
		}

		public static T Get<TApi>(HttpAuthenticator authenticator, string name) where TApi : IApi<T>
		{
			if (string.IsNullOrEmpty(name)) return null;
			var formattedName = name.ToLower();

			FillContainer<TApi>(authenticator);

			var value = Container.TryGetValue(formattedName, out var result) ? result : null;

			if (value != null) return value;

			var mKey = Container.Keys.FirstOrDefault(formattedName.Contains) ?? Container.Keys.FirstOrDefault(key => key.Contains(formattedName));
			return mKey != null ? Container[mKey] : null;
		}

		public static T GetDetailed<TApi>(HttpAuthenticator authenticator, string name) where TApi : IApi<T>
		{
			var item = Get<TApi>(authenticator, name);

			if (item == null) return null;
			if (item.WasDetailed) return item;

			var detailedItem = Api.Get(authenticator, item.Id);
			detailedItem.WasDetailed = true;
			Container.AddOrUpdate(name, detailedItem, (key, value) => value);

			return detailedItem;
		}

		public static T GetOrCreate<TApi>(HttpAuthenticator authenticator, string name, T puttingObject) where TApi : IApi<T>
		{
			if (string.IsNullOrEmpty(name)) return null;

			var item = Get<TApi>(authenticator, name);

			if (item == null)
			{
				item = Api.Create(authenticator, puttingObject);
				Container.AddOrUpdate(name, item, (key, type) => type);
			}

			return item;
		}
	}
}
