using System;
using System.Collections.Generic;

namespace PravoAdder.Api.Domain
{
	public class Expense : DatabaseEntityItem
	{
		public Project Project { get; set; }
		public DateTime Date { get; set; }
		public double Amount { get; set; }
		public string Description { get; set; }
		public List<string> Files { get; set; }
	}
}
