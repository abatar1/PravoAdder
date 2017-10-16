using System;

namespace PravoAdder.Domain
{
	public enum ReaderMode
	{
		All,
		Excel,
		XmlMap,
		ExcelRule,
		ExcelReference
	}

	[Flags]
	public enum ProcessType
	{
		Migration,
		Update,
		Sync,
		CleanAll,
		CleanByDate,
		CleanEmptyFolders,
		Test,
		Task,
		All
	}
}
