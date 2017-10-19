using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PravoAdder.Api.Domain
{
	public class ParticipantType : DatabaseEntityItem
	{
		public ParticipantType()
		{
		}

		public ParticipantType(object data) : base(data)
		{
			var dynData = data as dynamic;
			TypeName = dynData.TypeName.ToString();
			NameEn = dynData.TypeName.ToString();
		}

		public ParticipantType(string name, string id) : base(name, id)
		{
		}

		public string TypeName { get; }
		public string NameEn { get; }
	}
}
