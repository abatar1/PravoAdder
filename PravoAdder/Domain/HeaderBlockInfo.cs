using System.Collections.Generic;
using Newtonsoft.Json;

namespace PravoAdder.Domain
{
	public class HeaderBlockInfo
	{
		[FieldName("Название папки", "Folder Name")]
		public string ProjectFolder { get; set; }

		[FieldName("Название проекта")]
		public string ProjectGroup { get; set; }

		[FieldName("Название дела", "Case Name")]
		public string Name { get; set; }

		[FieldName("Ответственный", "Assignee")]
		public string Responsible { get; set; }

		[FieldName("Синхронизация")]
		public string CasebookNumber { get; set; }

		[FieldName("Тип дела", "Practice Area")]
		public string ProjectType { get; set; }

		[FieldName("Описание", "Description")]
		public string Description { get; set; }

		[FieldName("Архивное дело", "Is Archive Case")]
		public bool IsArchive { get; set; }

		[FieldName("Клиент", "Client")]
		public string Client { get; set; }

		[FieldName("Файлы", "Files")]
		public string FilesPath { get; set; }

		[JsonIgnore]
		public static Dictionary<string, int> Languages = new Dictionary<string, int> {["RU"] = 0, ["ENG"] = 1};

		[JsonIgnore]
		public static Dictionary<int, string> SystemNames = new Dictionary<int, string> { [0] = "Системный", [1] = "Summary" };

		public HeaderBlockInfo Format()
		{
			if (Name.Length > 350) Name = Name.Remove(350);
			if (ProjectFolder.Length > 100) ProjectFolder = ProjectFolder.Remove(100);
			return this;
		}
	}
}