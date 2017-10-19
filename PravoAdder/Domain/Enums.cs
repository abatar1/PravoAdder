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
<<<<<<< HEAD
		CreateTask,
		CreateParticipant,
		DistinctParticipant,
		DeleteAllParticipant,
=======
		CleanEmptyFolders,
		Test,
		Task,
		Participant,
>>>>>>> e06ccc4eb4c20f5b0a884c8c73b5e112fbac295a
		All
	}
}
