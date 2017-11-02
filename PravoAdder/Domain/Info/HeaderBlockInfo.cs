using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PravoAdder.Domain.Attributes;

namespace PravoAdder.Domain
{
	[Serializable]
	public class HeaderBlockInfo
	{
		[FieldName("Название папки", "Folder name")]
		public string ProjectFolder { get; set; }

		[FieldName("Название проекта")]
		public string ProjectGroup { get; set; }

		[FieldName("Название дела", "Case name")]
		public string Name { get; set; }

		[FieldName("Ответственный", "Assignee")]
		public string Responsible { get; set; }

		[FieldName("Синхронизация")]
		public string CasebookNumber { get; set; }

		[FieldName("Тип дела", "Practice area")]
		public string ProjectType { get; set; }

		[FieldName("Описание", "Description")]
		public string Description { get; set; }

		[FieldName("Архивное дело", "Is archive case")]
		public bool IsArchive { get; set; }

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