using System;

namespace PravoAdder
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Console.Title = "Pravo adder";
			var adder = new PravoAdder("config.json");
			adder.Run();
		}
	}
}