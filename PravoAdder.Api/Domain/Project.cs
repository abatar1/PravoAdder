using System;

namespace PravoAdder.Api.Domain
{
	public class Project : DatabaseEntityItem
	{
		public Project(string name, string id) : base(name, id)
		{
		}

		public Project(object data) : base(data)
		{
			var dynamicData = data as dynamic;
			CreationDate = DateTime.Parse(dynamicData.CreationDate.ToString());
		}

		public Project()
		{
		}

		public DateTime CreationDate { get; }
	}
}
