using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class BillingSettings : DatabaseEntityItem
	{
		public List<BillingRule> BillingRules { get; set; }
		public DictionaryItem SecondaryTaxMode { get; set; }
	}
}
