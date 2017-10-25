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
		DeleteCases,
		DeleteCasesByDate,
		CreateTask,
		CreateParticipants,
		EditParticipants,
		DistinctParticipants,
		DeleteParticipants,
		DeleteParticipantsByDate,
		Notes,
		All
	}
}
