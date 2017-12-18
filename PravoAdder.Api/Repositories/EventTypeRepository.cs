using System.Linq;
using PravoAdder.Api.Domain;

namespace PravoAdder.Api.Repositories
{
	public class EventTypeRepository : TemplateRepository<EventType, EventTypeApi>
	{
		private static EntityType _entityType;

		public static EventType GetOrPut(HttpAuthenticator authenticator, string itemName)
		{
			if (string.IsNullOrEmpty(itemName)) return null;
	
			var eventType = Get(authenticator, itemName);
			if (eventType == null)
			{
				if (_entityType == null) _entityType = ApiRouter.Bootstrap.GetEntityTypes(authenticator).First(e => e.Name.Equals("Event"));
				var dictionaryItem = new DictionaryItem{SystemName = _entityType.DictionarySystemName, Name = itemName };
				eventType = (EventType) ApiRouter.DictionaryItems.Create(authenticator, dictionaryItem);
				if (eventType == null) return null;

				Container.AddOrUpdate(itemName, eventType, (key, type) => type);
			}
			return eventType;
		}
	}
}
