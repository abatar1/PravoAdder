namespace PravoAdder
{
    internal class Program
    {      
        private static void Main(string[] args)
        {
            var adder = PravoAdder.Create("config.json");
            adder.Run();
        }
    }
}
