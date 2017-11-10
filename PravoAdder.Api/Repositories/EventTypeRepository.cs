using System.Linq;
using PravoAdder.Api.Domain;

namespace PravoAdder.Api.Repositories
{
	public class EventTypeRepository : TemplateRepository<EventType>
	{
		private static EntityType _entityType;

		public static EventType GetOrPut(HttpAuthenticator authenticator, string name)
		{
			var eventType = Get(authenticator, name);
			if (eventType == null)
			{
				_entityType = ApiRouter.Bootstrap.GetEntityTypes(authenticator).First(e => e.Name.Equals("Event"));
				eventType = (EventType) ApiRouter.Dictionary.Put(authenticator, _entityType.DictionarySystemName, name);
				Container.AddOrUpdate(name, eventType, (key, type) => type);
			}
			return eventType;
		}
	}
}
