using System.Collections.Generic;
using System.Linq;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Repositories;
using PravoAdder.Domain;

namespace PravoAdder.Readers
{
	public class BillingRuleCreator : Creator
	{
		private List<DictionaryItem> _calculationTypes;

		public override ICreatable Create(Table table, Row row, DatabaseEntityItem item = null)
		{
			var eventTypeName = table.GetValue(row, "Activity Type");
			var eventType = EventTypeRepository.GetOrPut(HttpAuthenticator, eventTypeName);

			var rateFieldValue = table.GetValue(row, "Rate");
			if (string.IsNullOrEmpty(rateFieldValue)) return null;
			var rate = double.Parse(rateFieldValue);

			var projectTypes = new List<ProjectType>();
			if (item == null)
			{
				var projectTypeName = table.GetValue(row, "Practice Area");
				if (string.IsNullOrEmpty(projectTypeName))
				{
					projectTypes.AddRange(ProjectTypeRepository.GetMany(HttpAuthenticator));
				}
				else
				{
					projectTypes.Add(ProjectTypeRepository.Get(HttpAuthenticator, projectTypeName));
				}				
			}

			if (_calculationTypes == null)
				_calculationTypes = ApiRouter.DefaultDictionaryItems.GetMany(HttpAuthenticator, "CalculationTypes");
			var calculationType = _calculationTypes.FirstOrDefault(c => c.Name.Equals("Hourly"));

			var billingRules = projectTypes.Select(projectType => new BillingRule(eventType, calculationType, rate, projectType)).ToList();
			if (billingRules.Count == 0) billingRules.Add(new BillingRule(eventType, calculationType, rate));

			return new BillingRuleWrapper {BillingRules = billingRules};
		}

		public BillingRuleCreator(HttpAuthenticator httpAuthenticator, Settings settings) : base(httpAuthenticator, settings)
		{
		}
	}
}
