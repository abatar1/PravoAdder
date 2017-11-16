using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class EventTypeApi : IApi<EventType>
	{
		public List<EventType> GetMany(HttpAuthenticator authenticator, string optional = null)
		{
			return ApiHelper.GetItems<EventType>(authenticator, "EventTypes/GetEventTypes", HttpMethod.Post);
		}

		public EventType Get(HttpAuthenticator authenticator, string parameter)
		{
			throw new System.NotImplementedException();
		}

		public EventType Create(HttpAuthenticator authenticator, EventType puttingObject)
		{
			throw new System.NotImplementedException();
		}
	}
}
