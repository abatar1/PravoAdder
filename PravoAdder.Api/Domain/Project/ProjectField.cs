using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class ProjectField : DatabaseEntityItem, ICreatable
	{
		public string PlaceholderText { get; set; }
		public ProjectFieldFormat ProjectFieldFormat { get; set; }
		public List<CalculationFormula> CalculationFormulas { get; set; }

		public override string ToString() => Name;
	}
}
