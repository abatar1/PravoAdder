namespace PravoAdder.Api.Domain
{
	public class BilledTimes : Bill
	{
		public int Time { get; set; }
		public double Rate { get; set; }
		public double Total { get; set; }
		public bool DiscountPercentage { get; set; }
		public bool HasTax { get; set; }
		public bool HasSecondaryTax { get; set; }
		public RateCalculationType RateCalculationType { get; set; }
	}
}
