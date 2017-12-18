using System.Collections.Generic;
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

		public List<Timing> GetMany(HttpAuthenticator authenticator, string projectId)
		{
			var parameter = ApiHelper.CreateParameters(("ProjectId", projectId));
			return ApiHelper.GetItems<Timing>(authenticator, "Timing/GetTimings", HttpMethod.Post, parameter);
		}

		public TimeLog Get(HttpAuthenticator authenticator, string entityId)
		{
			var parameter = ApiHelper.CreateParameters(("EntityId", entityId));
			return ApiHelper.GetItem<TimeLog>(authenticator, "TimeLogs/GetByEntity", HttpMethod.Get, parameter);
		}
	}
}
