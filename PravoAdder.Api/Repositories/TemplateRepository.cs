using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PravoAdder.Api.Domain;

namespace PravoAdder.Api.Repositories
{
	public abstract class TemplateRepository<TEntity, TRoute> where TEntity : DatabaseEntityItem
	{
		protected static ConcurrentDictionary<string, TEntity> Container;

		private static IApi<TEntity> _api;

		protected static IApi<TEntity> Api => _api ?? (_api = (IApi<TEntity>) Activator.CreateInstance(typeof(TRoute)));

		public static bool IsContainerEmpty;
		private static readonly object IsContainerEmptyLock = new object();	

		private static void FillContainer(HttpAuthenticator authenticator, string optional = null)
		{
			lock (IsContainerEmptyLock)
			{
				IsContainerEmpty = Container == null;
			}

			if (IsContainerEmpty)
			{
				var container = Api.GetMany(authenticator, optional)
					.Select(x => new KeyValuePair<string, TEntity>(x.Name?.ToLower() ?? x.DisplayName.ToLower(), x))
					.GroupBy(x => x.Key)
					.Select(g => g.First())
					.ToList();
				if (optional != null)
				{
					if (Container == null) Container = new ConcurrentDictionary<string, TEntity>();
					container.ForEach(c => Container.AddOrUpdate(c.Key, c.Value, (s, arg2) => arg2));
				}
				else
				{
					Container = new ConcurrentDictionary<string, TEntity>(container);
				}				
			}
		}

		public static IEnumerable<TEntity> GetMany(HttpAuthenticator authenticator, string optional = null)
		{
			FillContainer(authenticator, optional);
			return Container?.Values.ToList() ?? new List<TEntity>();
		}

		public static IEnumerable<Lazy<TEntity>> GetManyDetailed(HttpAuthenticator authenticator, string optional = null)
		{
			FillContainer(authenticator, optional);
			return Container?.Values.Select(x => new Lazy<TEntity>(() => GetDetailed(authenticator, x.Name))).ToList() ??
			       new List<Lazy<TEntity>>();
		}

		public static TEntity Get(HttpAuthenticator authenticator, string name)
		{
			if (string.IsNullOrEmpty(name)) return null;
			var formattedName = name.ToLower();

			FillContainer(authenticator);

			var value = Container.TryGetValue(formattedName, out var result) ? result : null;

			if (value != null) return value;

			var mKey = Container.Keys.FirstOrDefault(formattedName.Contains) ?? Container.Keys.FirstOrDefault(key => key.Contains(formattedName));
			return mKey != null ? Container[mKey] : null;
		}

		public static TEntity GetDetailed(HttpAuthenticator authenticator, string name)
		{
			var item = Get(authenticator, name);

			if (item == null) return null;
			if (item.WasDetailed) return item;

			var detailedItem = Api.Get(authenticator, item.Id);
			detailedItem.WasDetailed = true;
			Container.AddOrUpdate(name.ToLower(), detailedItem, (key, value) => detailedItem);

			return detailedItem;
		}

		public static TEntity Create(HttpAuthenticator authenticator, TEntity puttingObject)
		{
			var item = Api.Create(authenticator, puttingObject);
		    var name = item.Name?.ToLower() ?? item.DisplayName?.ToLower() ?? throw new ArgumentException("Cannot response rule for reading item name.");
			Container.AddOrUpdate(name, item, (key, value) => item);
			return item;
		}

		public static TEntity GetOrCreate(HttpAuthenticator authenticator, string name, TEntity puttingObject)
		{
			if (string.IsNullOrEmpty(name)) return null;

			return Get(authenticator, name) ?? Create(authenticator, puttingObject);
		}		
	}
}
