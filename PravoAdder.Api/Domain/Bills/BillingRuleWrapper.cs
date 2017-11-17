using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class BillingRuleWrapper : ICreatable
	{
		public List<BillingRule> BillingRules { get; set; }
	}
}
