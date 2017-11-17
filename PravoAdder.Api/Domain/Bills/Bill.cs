namespace PravoAdder.Api.Domain
{
	public class Bill : DatabaseEntityItem, ICreatable
	{
		public Project Project { get; set; }
		public Participant Client { get; set; }
		public BillStatus BillStatus { get; set; }
	}
}
