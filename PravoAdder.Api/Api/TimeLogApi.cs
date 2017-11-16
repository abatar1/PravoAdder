using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class TimeLogApi
	{
		public TimeLog Create(HttpAuthenticator authenticator, TimeLog timeLog)
		{
			return ApiHelper.GetItem<TimeLog>(authenticator, "TimeLogs/Add", HttpMethod.Post, timeLog);
		}
	}
}
