using System;

namespace PravoAdder.Domain
{
	public enum ReaderMode
	{
		All,
		Excel,
		XmlMap
	}

	[Flags]
	public enum ProcessType
	{
		Migration,
		Syncronization,
		CleanAll,
		CleanByDate,
		CleanEmptyFolders,
		Test,
		All
	}
}
