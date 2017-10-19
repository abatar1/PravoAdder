namespace PravoAdder
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			var engine = new Engine();
			engine.Initialize(args).Run();
		}
	}
}