using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class CalendarApi : IApi<Calendar>
	{
		public List<Calendar> GetMany(HttpAuthenticator authenticator, string optional = null)
		{
			return ApiHelper.GetItems<Calendar>(authenticator, "Calendar/GetEventCalendars", HttpMethod.Post);
		}

		public Calendar Get(HttpAuthenticator authenticator, string parameter)
		{
			throw new System.NotImplementedException();
		}

		public Calendar Create(HttpAuthenticator authenticator, Calendar puttingObject)
		{
			throw new System.NotImplementedException();
		}
	}
}
