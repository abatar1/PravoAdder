﻿namespace PravoAdder.Api
{
	public class Api
	{
		public static ProjectsApi Projects;
		public static ProjectFoldersApi ProjectFolders;
		public static ProjectGroupsApi ProjectGroups;
		public static ProjectTypesApi ProjectTypes;
		public static ParticipantsApi Participants;
		public static DictionaryApi Dictionary;
		public static CasebookApi Casebook;
		public static ProjectCustomValuesApi ProjectCustomValues;
		public static CalculationFormulasApi CalculationFormulas;
		public static ResponsiblesApi Responsibles;

		public static int PageSize = 50;

		static Api()
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
		}
	}
}
