namespace PravoAdder.Api.Domain
{
	public class BillingRule
	{
		public EventType EventType { get; set; }
		public DictionaryItem CalculationType { get; set; }
		public ProjectType ProjectType { get; set; }
		public double Rate { get; set; }

		public BillingRule(EventType eventType, DictionaryItem calculationType, double rate, ProjectType projectType = null)
		{
			EventType = eventType;
			CalculationType = calculationType;
			Rate = rate;
			ProjectType = projectType;
		}
	}
}
