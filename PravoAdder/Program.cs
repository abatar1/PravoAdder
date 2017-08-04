using System;
using System.Linq;
namespace PravoAdder
{
    internal class Program
    {      
        private static void Main(string[] args)
        {
            var adder = new PravoAdder("blocksInfo.json", "config.json");
            adder.Run();                     
        }
    }
}
