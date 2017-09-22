using System;

namespace PravoAdder
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var engine = new Engine();
			engine.Initialize(new [] { "-t", "Migration" }).Run();

			Console.WriteLine("Processed.");
			Console.ReadKey();
		}
	}
}