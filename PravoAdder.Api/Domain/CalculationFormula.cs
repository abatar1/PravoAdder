namespace PravoAdder.Api.Domain
{
	public class CalculationFormula : DatabaseEntityItem
	{
		public CalculationFormula()
		{
		}

		public CalculationFormula(object data) : base(data)
		{
		}

		public CalculationFormula(string name, string id) : base(name, id)
		{
		}
	}
}
