namespace PravoAdder.Domain.Info
{
	public class HeaderBlockInfo
	{
		[FieldName("Название папки")]
		public string FolderName { get; set; }

		[FieldName("Название проекта")]
		public string ProjectGroupName { get; set; }

		[FieldName("Название дела")]
		public string ProjectName { get; set; }

		[FieldName("Ответственный")]
		public string ResponsibleName { get; set; }

		[FieldName("Синхронизация")]
		public string SynchronizationNumber { get; set; }

		[FieldName("Тип дела")]
		public string ProjectTypeName { get; set; }

		[FieldName("Описание")]
		public string Description { get; set; } = "Created automatically";
	}
}