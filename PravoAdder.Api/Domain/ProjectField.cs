using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class ProjectField
	{
		public string Name { get; set; }
		public ProjectFieldFormat ProjectFieldFormat { get; set; }
		public List<CalculationFormula> CalculationFormulas { get; set; }
		public override string ToString() => Name;
	}
}
