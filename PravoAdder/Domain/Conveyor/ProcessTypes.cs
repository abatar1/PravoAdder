using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PravoAdder.Domain
{
	//
	// Naming rule {Entity}{Action}{Many %if processing many sources%}
	//
	public class ProcessTypes
	{
		public static ProcessType CaseCreate { get; } = new ProcessType("CaseCreate", true);
		public static ProcessType CaseUpdate { get; } = new ProcessType("CaseUpdate", true);
		public static ProcessType CaseSync { get; } = new ProcessType("CaseSync", true);
		public static ProcessType CaseDelete { get; } = new ProcessType("CaseDelete", false);
		public static ProcessType CaseUpdateSettings { get; } = new ProcessType("CaseUpdateSettings", true);
		public static ProcessType CaseRename { get; } = new ProcessType("CaseRename", true);
		public static ProcessType CaseUnload { get; } = new ProcessType("CaseUnload", false);
		public static ProcessType CaseDeleteByDate { get; } = new ProcessType("CaseDeleteByDate", false);
		public static ProcessType CaseTypeCreate { get; } = new ProcessType("CaseTypeCreate", true);

		public static ProcessType ParticipantAttach { get; } = new ProcessType("ParticipantAttach", true);
		public static ProcessType ParticipantCreate { get; } = new ProcessType("ParticipantCreate", true);
		public static ProcessType ParticipantEdit { get; } = new ProcessType("ParticipantEdit", true);
		public static ProcessType ParticipantEditByKey { get; } = new ProcessType("ParticipantEditByKey", true);
		public static ProcessType ParticipantDistinct { get; } = new ProcessType("ParticipantDistinct", false);
		public static ProcessType ParticipantDelete { get; } = new ProcessType("ParticipantDelete", false);
		public static ProcessType ParticipantDeleteByDate { get; } = new ProcessType("ParticipantDeleteByDate", false);

		public static ProcessType EventCreate { get; } = new ProcessType("EventCreate", true);
		public static ProcessType EventDelete { get; } = new ProcessType("EventDelete", false);

		public static ProcessType ExpenseCreate { get; } = new ProcessType("ExpenseCreate", false);
		public static ProcessType ExpenseCreateMany { get; } = new ProcessType("ExpenseCreateMany", true);

		public static ProcessType HeaderAnalyze { get; } = new ProcessType("HeaderAnalyze", true);
		public static ProcessType TaskCreate { get; } = new ProcessType("TaskCreate", true);
		public static ProcessType ProjectFieldCreate { get; } = new ProcessType("ProjectFieldCreate", true);
		public static ProcessType VisualBlockLineAdd { get; } = new ProcessType("VisualBlockLineAdd", true);
		public static ProcessType DictionaryCreate { get; } = new ProcessType("DictionaryCreate", true);
		public static ProcessType BillingRuleUpdate { get; } = new ProcessType("BillingRuleUpdate", false);
		public static ProcessType NoteCreate { get; } = new ProcessType("NoteCreate", false);
		public static ProcessType BillCreate { get; } = new ProcessType("BillCreate", false);
		public static ProcessType DocumentUpload { get; } = new ProcessType("DocumentUpload", true);

		private static readonly IEnumerable<PropertyInfo> Properties;

		static ProcessTypes()
		{
			Properties = typeof(ProcessTypes).GetProperties();
		}

		public static ProcessType GetByName(string name)
		{
			var property = Properties.FirstOrDefault(p => p.Name.Equals(name));
			return (ProcessType) property?.GetValue(typeof(ProcessType), null);
		}
	}
}
