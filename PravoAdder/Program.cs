namespace PravoAdder
{
    internal class Program
    {      
        private static void Main(string[] args)
        {
            var adder = new PravoAdder("config.json");
            adder.Run();                     
        }
    }
}
