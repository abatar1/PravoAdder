namespace PravoAdder.Api.Domain
{
	public class TaskState : DatabaseEntityItem
	{
		public TaskState(string name, string id, string sysName)
		{
			Name = name;
			Id = id;
			SysName = sysName;
		}
	}
}
