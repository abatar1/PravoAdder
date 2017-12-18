using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;
using System.Linq;

namespace PravoAdder.Api
{
	public class EventApi : IApi<Event>
	{
		public Event Get(HttpAuthenticator authenticator, string eventId)
		{
			var parameter = ApiHelper.CreateParameters(("EventId", eventId));
			return ApiHelper.GetItem<Event>(authenticator, "events", HttpMethod.Get, parameter);
		}

		public Event Create(HttpAuthenticator authenticator, Event newEvent)
		{
			return ApiHelper.GetItem<Event>(authenticator, "events", HttpMethod.Put, newEvent);
		}

		public List<Event> GetMany(HttpAuthenticator authenticator, string optional = null)
		{
			return ApiHelper.GetItems<GroupWrapper>(authenticator, "feed/Groups", HttpMethod.Post)
				.SelectMany(w => w.Result)
				.SelectMany(w => w.Result)
				.Where(e => e.EntityName.Equals("Event"))
				.Select(w => (Event) w)
				.ToList();
		}

		public void Delete(HttpAuthenticator authenticator, string id)
		{
			var parameters = ApiHelper.CreateParameters(("Id", id));
			ApiHelper.SendItem(authenticator, "events", HttpMethod.Delete, parameters);
		}
	}
}
