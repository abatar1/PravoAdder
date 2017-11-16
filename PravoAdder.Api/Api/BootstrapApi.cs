using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class BootstrapApi
	{
		public dynamic Get(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.GetItem(httpAuthenticator, "bootstrap/GetBootstrap", HttpMethod.Get, new Dictionary<string, string>());
		}

		public dynamic GetShell(HttpAuthenticator httpAuthenticator)
		{
			return ApiHelper.GetItem(httpAuthenticator, "bootstrap/GetShellBootstrap", HttpMethod.Get, new Dictionary<string, string>());
		}

		public List<LineType> GetLineTypes(HttpAuthenticator httpAuthenticator)
		{
			var bootstrap = ApiRouter.Bootstrap.GetShell(httpAuthenticator);
			IEnumerable<dynamic> participantTypes = bootstrap["ProjectFieldFormats"]["LineTypes"];
			return participantTypes.Select(o => (LineType)JsonConvert.DeserializeObject<LineType>(o.ToString())).ToList();
		}

		public List<ProjectFieldFormat> GetFieldTypes(HttpAuthenticator httpAuthenticator)
		{
			var bootstrap = ApiRouter.Bootstrap.GetShell(httpAuthenticator);
			IEnumerable<dynamic> participantTypes = bootstrap["ProjectFieldFormats"]["ProjectFieldFormats"];
			return participantTypes.Select(o => (ProjectFieldFormat)JsonConvert.DeserializeObject<ProjectFieldFormat>(o.ToString())).ToList();
		}

		public List<EntityType> GetEntityTypes(HttpAuthenticator httpAuthenticator)
		{
			var bootstrap = ApiRouter.Bootstrap.GetShell(httpAuthenticator);
			IEnumerable<dynamic> participantTypes = bootstrap["ProjectFieldFormats"]["EntityTypes"];
			return participantTypes.Select(o => (EntityType)JsonConvert.DeserializeObject<EntityType>(o.ToString())).ToList();
		}

		public List<ActivityTag> GetActivityTags(HttpAuthenticator httpAuthenticator)
		{
			var bootstrap = ApiRouter.Bootstrap.Get(httpAuthenticator);
			IEnumerable<dynamic> activityTags = bootstrap["CaseMap.Modules.Main"]["CaseMap.Modules.Activities"]["ActivityTags"];
			return activityTags.Select(o => (ActivityTag) JsonConvert.DeserializeObject<ActivityTag>(o.ToString())).ToList();
		}

		public List<ParticipantType> GetParticipantTypes(HttpAuthenticator httpAuthenticator)
		{
			var bootstrap = ApiRouter.Bootstrap.Get(httpAuthenticator);
			IEnumerable<dynamic> participantTypes = bootstrap["CaseMap.Modules.Main"]["CaseMap.Modules.Participants"]["ParticipantTypes"];
			return participantTypes.Select(o => (ParticipantType)JsonConvert.DeserializeObject<ParticipantType>(o.ToString())).ToList();
		}
	}
}
