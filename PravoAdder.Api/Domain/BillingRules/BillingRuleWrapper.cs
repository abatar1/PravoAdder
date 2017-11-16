using System.Collections.Generic;

namespace PravoAdder.Api.Domain.Other
{
	public class BillingRuleWrapper : ICreatable
	{
		public List<BillingRule> BillingRules { get; set; }
	}
}
