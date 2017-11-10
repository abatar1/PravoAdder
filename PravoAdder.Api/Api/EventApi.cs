using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api.Api
{
	public class EventApi : IGetMany<EventType>
	{
		public List<EventType> GetMany(HttpAuthenticator authenticator, string optional = null)
		{
			return ApiHelper.GetItems<EventType>(authenticator, "EventTypes/GetEventTypes", HttpMethod.Post);
		}

		public EventType Get(HttpAuthenticator authenticator, string parameter)
		{
			throw new System.NotImplementedException();
		}

		public Event Create(HttpAuthenticator authenticator, Event newEvent)
		{
			return ApiHelper.GetItem<Event>(authenticator, "events", HttpMethod.Put, newEvent);
		}
	}
}
