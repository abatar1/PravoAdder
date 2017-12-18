namespace PravoAdder
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using (var engine = new GuiEngine("instance_enviroment.json"))
			{
				engine.StartGui();
			}
		}
	}
}