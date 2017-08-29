using System;

namespace PravoAdder
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Console.Title = "Pravo adder";
			var adder = PravoAdder.Create("config.json");
			adder.Run();
		}
	}
}