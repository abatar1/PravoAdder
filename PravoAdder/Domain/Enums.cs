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
		Analyze,
		CleanAll,
		CleanByDate,
		CreateTask,
		CreateParticipant,
		DistinctParticipant,
		DeleteAllParticipant,
		DeleteParticipantByDate,
		Notes,
		All
	}
}
