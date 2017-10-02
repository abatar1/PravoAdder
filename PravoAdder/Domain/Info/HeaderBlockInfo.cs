namespace PravoAdder.Domain
{
	public class HeaderBlockInfo
	{
		[FieldName("Название папки")]
		public string FolderName { get; set; }

		[FieldName("Название проекта")]
		public string ProjectGroupName { get; set; }

		[FieldName("Название дела", "Case name")]
		public string ProjectName { get; set; }

		[FieldName("Ответственный", "Assignee")]
		public string ResponsibleName { get; set; }

		[FieldName("Синхронизация")]
		public string SynchronizationNumber { get; set; }

		[FieldName("Тип дела", "Practice area")]
		public string ProjectTypeName { get; set; }

		[FieldName("Описание")]
		public string Description { get; set; }

		[FieldName("Номер дела")]
		public string ProjectNumber { get; set; }
	}
}