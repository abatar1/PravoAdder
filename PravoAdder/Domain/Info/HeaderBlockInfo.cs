using PravoAdder.Domain.Attributes;

namespace PravoAdder.Domain
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
		public string Description { get; set; }

		[FieldName("Номер дела")]
		public string ProjectNumber { get; set; }


		public HeaderBlockInfo Format()
		{
			if (ProjectName.Length > 350) ProjectName = ProjectName.Remove(350);
			if (FolderName.Length > 100) FolderName = FolderName.Remove(100);
			return this;
		}
	}
}