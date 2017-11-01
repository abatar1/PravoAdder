﻿using System;

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
		UnloadCases,
		DeleteCasesByDate,
		CreateTask,
		CreateParticipants,
		EditParticipantsByKey,
		EditParticipants,
		DistinctParticipants,
		DeleteParticipants,
		DeleteParticipantsByDate,
		CreateProjectField,
		AddVisualBlockRow,
		Notes,
		All
	}
}
