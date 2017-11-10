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
		protected static IGetMany<T> Router;

		public static bool IsContainerEmpty;
		private static readonly object IsContainerEmptyLock = new object();	

		private static void FillContainer(HttpAuthenticator authenticator)
		{
			lock (IsContainerEmptyLock)
			{
				IsContainerEmpty = Container == null;
			}

			if (IsContainerEmpty)
			{
				var routerType = typeof(ApiRouter).GetFields()
					.FirstOrDefault(pf => pf.FieldType.GetInterfaces()
						.Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IGetMany<>) &&
							        x.GetGenericArguments()[0] == typeof(T)))?.FieldType;
				
				if (routerType == null) throw new NotImplementedException($"Не существует маршрута для Api вызова {typeof(T).Name}");

				Router = (IGetMany<T>) Activator.CreateInstance(routerType);

				var container = Router.GetMany(authenticator).Select(a => new KeyValuePair<string, T>(a.Name, a));
				Container = new ConcurrentDictionary<string, T>(container);
			}
		}

		public static List<T> GetMany(HttpAuthenticator authenticator)
		{
			FillContainer(authenticator);
			return Container?.Values.ToList() ?? new List<T>();
		}

		public static T Get(HttpAuthenticator authenticator, string name)
		{
			FillContainer(authenticator);

			var value = Container.TryGetValue(name, out var result) ? result : null;

			if (value != null) return value;

			var mKey = Container.Keys.FirstOrDefault(name.Contains);
			return mKey != null ? Container[mKey] : null;
		}

		public static T GetDetailed(HttpAuthenticator authenticator, string name)
		{
			var project = Get(authenticator, name);

			if (project == null) return null;
			if (project.WasDetailed) return project;

			var detailedItem = Router.Get(authenticator, project.Id);
			detailedItem.WasDetailed = true;
			Container.AddOrUpdate(name, detailedItem, (key, item) => item);

			return detailedItem;
		}
	}
}
