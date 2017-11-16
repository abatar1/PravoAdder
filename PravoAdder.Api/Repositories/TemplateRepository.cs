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

		private static void FillContainer<TRouter>(HttpAuthenticator authenticator, string optional = null) where TRouter : IApi<T>
		{
			lock (IsContainerEmptyLock)
			{
				IsContainerEmpty = Container == null;
			}

			if (IsContainerEmpty)
			{
				Api = (IApi<T>) Activator.CreateInstance(typeof(TRouter));

				var container = Api.GetMany(authenticator, optional).Select(a => new KeyValuePair<string, T>(a.Name?.ToLower() ?? a.DisplayName.ToLower(), a)).ToList();
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

		public static List<T> GetMany<TRouter>(HttpAuthenticator authenticator, string optional = null) where TRouter : IApi<T>
		{
			FillContainer<TRouter>(authenticator, optional);
			return Container?.Values.ToList() ?? new List<T>();
		}

		public static T Get<TRouter>(HttpAuthenticator authenticator, string name) where TRouter : IApi<T>
		{
			if (string.IsNullOrEmpty(name)) return null;

			FillContainer<TRouter>(authenticator);

			var value = Container.TryGetValue(name.ToLower(), out var result) ? result : null;

			if (value != null) return value;

			var mKey = Container.Keys.FirstOrDefault(name.Contains) ?? Container.Keys.FirstOrDefault(key => key.Contains(name));
			return mKey != null ? Container[mKey] : null;
		}

		public static T GetDetailed<TRouter>(HttpAuthenticator authenticator, string name) where TRouter : IApi<T>
		{
			var item = Get<TRouter>(authenticator, name);

			if (item == null) return null;
			if (item.WasDetailed) return item;

			var detailedItem = Api.Get(authenticator, item.Id);
			detailedItem.WasDetailed = true;
			Container.AddOrUpdate(name, detailedItem, (key, value) => value);

			return detailedItem;
		}

		public static T GetOrCreate<TRouter>(HttpAuthenticator authenticator, string name, T puttingObject) where TRouter : IApi<T>
		{
			if (string.IsNullOrEmpty(name)) return null;

			var item = Get<TRouter>(authenticator, name);

			if (item == null)
			{
				item = Api.Create(authenticator, puttingObject);
				Container.AddOrUpdate(name, item, (key, type) => type);
			}

			return item;
		}
	}
}
