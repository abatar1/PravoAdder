using System;

namespace PravoAdder.Api.Domain
{
	public class Project : DatabaseEntityItem
	{
		public Project(string name, string id, string typeId, string groupId, bool isArchive = false) : base(name, id)
		{
			ProjectTypeId = string.IsNullOrEmpty(typeId) ? null : typeId;
			ProjectGroupId = string.IsNullOrEmpty(groupId) ? null : groupId;
			IsArchive = isArchive;
		}

		public Project(object data) : base(data)
		{
			var dynamicObj = data as dynamic;
			CreationDate = DateTime.Parse(dynamicObj.CreationDate.ToString());
			ProjectGroupId = dynamicObj.ProjectGroupId;
			ProjectTypeId = dynamicObj.ProjectTypeId;
			IsArchive = dynamicObj.IsArchive ?? false;
		}

		public Project()
		{
		}

		public DateTime CreationDate { get; }
		public string ProjectTypeId { get; }
		public string ProjectGroupId { get; }
		public bool IsArchive { get; }
	}
}
