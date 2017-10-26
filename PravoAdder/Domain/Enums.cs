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
		AttachParticipant,
		DeleteCases,
		RenameCases,
		DeleteCasesByDate,
		CreateTask,
		CreateParticipants,
		EditParticipantsByKey,
		EditParticipants,
		DistinctParticipants,
		DeleteParticipants,
		DeleteParticipantsByDate,
		Notes,
		All
	}
}
