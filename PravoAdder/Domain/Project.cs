using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PravoAdder.Domain
{
    public class Project
    {
        public ProjectFolder ProjectFolder { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
