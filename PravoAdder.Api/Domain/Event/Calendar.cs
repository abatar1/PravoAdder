using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PravoAdder.Api.Domain
{
	public class Calendar : DatabaseEntityItem
	{
		public bool IsSmart { get; set; }
		public CalendarColor CalendarColor { get; set; }
	}
}
