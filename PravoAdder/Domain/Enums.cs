using System;

namespace PravoAdder.Domain
{
	public enum ReadingMode
	{
		All,
		Excel,
		XmlMap,
		ExcelRule,
		ExcelReference
	}

	//
	// Naming rule {Entity}{Action}
	//
	[Flags]
	public enum ProcessType
	{
		CaseCreate,
		CaseUpdate,
		CaseSync,
		CaseDelete,
		CaseUpdateSettings,
		CaseRename,
		CaseUnload,
		CaseDeleteByDate,
		HeaderAnalyze,		
		TaskCreate,
		ParticipantAttach,
		ParticipantCreate,
		ParticipantEditByKey,
		ParticipantEdit,
		ParticipantDistinct,
		ParticipantDelete,
		ParticipantDeleteByDate,
		ProjectFieldCreate,
		VisualBlockLineAdd,
		DictionaryCreate,
		CaseTypeCreate,
		EventCreate,
		EventDelete,
		ExpenseCreate,
		BillingRuleUpdate,
		NoteCreate,
		All
	}
}
