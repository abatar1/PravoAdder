using System;
using PravoAdder.Domain.Attributes;

namespace PravoAdder.Domain
{
	[Serializable]
	public class HeaderBlockInfo
	{
		[FieldName("Название папки", "Folder name")]
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

		[FieldName("Описание", "Description")]
		public string Description { get; set; }

		[FieldName("Номер дела")]
		public string ProjectNumber { get; set; }

		[FieldName("Архивное дело", "Is archive case")]
		public bool IsArchive { get; set; }


		public HeaderBlockInfo Format()
		{
			if (ProjectName.Length > 350) ProjectName = ProjectName.Remove(350);
			if (FolderName.Length > 100) FolderName = FolderName.Remove(100);
			return this;
		}
	}
}