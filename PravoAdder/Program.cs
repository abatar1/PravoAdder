using System;

namespace PravoAdder
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Console.Title = "Pravo.Add";
			var adder = new PravoAdder("config.json");
			adder.Run();
		}
	}
}