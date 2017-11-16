using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class ProjectSettings : DatabaseEntityItem
	{
		public string ProjectId { get; set; }
		public dynamic Budget { get; set; }
		public dynamic SharedUsersAndGroups { get; set; }
		public List<dynamic> ProjectClients { get; set; }
		public List<BillingRule> BillingRules { get; set; }
		public dynamic ProjectSettingsBilling { get; set; }
		public List<dynamic> DeletedClients { get; set; }
	}
}
