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
		CreateTask,
		CreateParticipant,
		DistinctParticipant,
		DeleteAllParticipant,
		DeleteParticipantByDate,
		All
	}
}
