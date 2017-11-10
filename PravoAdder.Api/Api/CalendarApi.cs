using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api.Api
{
	public class CalendarApi : IGetMany<Calendar>
	{
		public List<Calendar> GetMany(HttpAuthenticator authenticator, string optional = null)
		{
			return ApiHelper.GetItems<Calendar>(authenticator, "Calendar/GetEventCalendars", HttpMethod.Post);
		}

		public Calendar Get(HttpAuthenticator authenticator, string parameter)
		{
			throw new System.NotImplementedException();
		}
	}
}
