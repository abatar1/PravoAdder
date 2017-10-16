namespace PravoAdder.Api.Domain
{
	public class TaskState : DatabaseEntityItem
	{
		public TaskState()
		{
		}

		public TaskState(object data) : base(data)
		{
		}

		public TaskState(string name, string id, string sysName) : base(name, id)
		{
		    SysName = sysName;
		}
	}
}
