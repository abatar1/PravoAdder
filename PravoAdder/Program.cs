using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PravoAdder
{
    class Program
    {
        static void Main(string[] args)
        {
            var excelReader = new ExcelReader("test.xlsx");
            var filler = new DBFiller();
            filler.Authentication("admin@pravo.ru", "123123");
            filler.AddProjectAsync("ctest");
        }
    }
}
