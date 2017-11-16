﻿using System.Collections.Generic;
using System.Linq;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Domain.Other;
using PravoAdder.Api.Repositories;
using PravoAdder.Domain;

namespace PravoAdder.Readers
{
	public class BillingRulesCreator : ICreator
	{
		private List<DictionaryItem> _calculationTypes;

		public BillingRulesCreator(HttpAuthenticator httpAuthenticator)
		{
			HttpAuthenticator = httpAuthenticator;
		}

		public HttpAuthenticator HttpAuthenticator { get; }

		public ICreatable Create(Row header, Row row, DatabaseEntityItem item = null)
		{
			var eventTypeName = Table.GetValue(header, row, "Activity Type");
			var eventType = EventTypeRepository.GetOrPut(HttpAuthenticator, eventTypeName);

			var rateFieldValue = Table.GetValue(header, row, "Rate");
			if (string.IsNullOrEmpty(rateFieldValue)) return null;
			var rate = double.Parse(rateFieldValue);

			var projectTypes = new List<ProjectType>();
			if (item == null)
			{
				var projectTypeName = Table.GetValue(header, row, "Practice Area");
				if (string.IsNullOrEmpty(projectTypeName))
				{
					projectTypes.AddRange(ProjectTypeRepository.GetMany<ProjectTypesApi>(HttpAuthenticator));
				}
				else
				{
					projectTypes.Add(ProjectTypeRepository.Get<ProjectTypesApi>(HttpAuthenticator, projectTypeName));
				}				
			}

			if (_calculationTypes == null)
				_calculationTypes = ApiRouter.DefaultDictionaryItems.GetMany(HttpAuthenticator, "CalculationTypes");
			var calculationType = _calculationTypes.FirstOrDefault(c => c.Name.Equals("Hourly"));

			var billingRules = projectTypes.Select(projectType => new BillingRule(eventType, calculationType, rate, projectType)).ToList();
			if (billingRules.Count == 0) billingRules.Add(new BillingRule(eventType, calculationType, rate));

			return new BillingRuleWrapper {BillingRules = billingRules};
		}
	}
}
