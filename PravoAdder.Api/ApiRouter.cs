using PravoAdder.Api;
using PravoAdder.Api.Api;

namespace PravoAdder
{
	public class ApiRouter
	{
		public static ProjectsApi Projects;
		public static ProjectFoldersApi ProjectFolders;
		public static ProjectGroupsApi ProjectGroups;
		public static ProjectTypesApi ProjectTypes;
		public static ParticipantsApi Participants;
		public static DictionaryApi Dictionary;
		public static DictionaryItemApi DictionaryItems;
		public static DefaultDictionaryItemsApi DefaultDictionaryItems;
		public static CasebookApi Casebook;
		public static ProjectCustomValuesApi ProjectCustomValues;
		public static CalculationFormulasApi CalculationFormulas;
		public static ResponsiblesApi Responsibles;
		public static CurrenciesApi Currencies;
		public static TaskApi Task;
		public static BootstrapApi Bootstrap;
		public static NotesApi Notes;
		public static ProjectFieldsApi ProjectFields;
		public static VisualBlockApi VisualBlocks;
		public static EventTypeApi EventTypes;
		public static EventApi Events;
		public static CalendarApi Calendars;
		public static TimeLogApi TimeLogs;
		public static ProjectSettingsApi ProjectSettings;
		public static ExpensesApi Expenses;
		public static BillingSettingsApi BillingSettings;

		public static int PageSize = 50;

		static ApiRouter()
		{
			Projects = new ProjectsApi();
			ProjectFolders = new ProjectFoldersApi();
			ProjectGroups = new ProjectGroupsApi();
			ProjectTypes = new ProjectTypesApi();
			Participants = new ParticipantsApi();
			Dictionary = new DictionaryApi();
			Casebook = new CasebookApi();
			ProjectCustomValues = new ProjectCustomValuesApi();
			CalculationFormulas = new CalculationFormulasApi();
			Responsibles = new ResponsiblesApi();
			Currencies = new CurrenciesApi();
			Task = new TaskApi();
			Bootstrap = new BootstrapApi();
			Notes = new NotesApi();
			ProjectFields = new ProjectFieldsApi();
			VisualBlocks = new VisualBlockApi();
			EventTypes = new EventTypeApi();
			Calendars = new CalendarApi();
			TimeLogs = new TimeLogApi();
			Events = new EventApi();
			ProjectSettings = new ProjectSettingsApi();
			DictionaryItems = new DictionaryItemApi();
			DefaultDictionaryItems = new DefaultDictionaryItemsApi();
			Expenses = new ExpensesApi();
			BillingSettings = new BillingSettingsApi();
		}
	}
}
