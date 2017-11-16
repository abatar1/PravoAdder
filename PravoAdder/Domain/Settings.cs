using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;
using PravoAdder.Domain.Attributes;

namespace PravoAdder.Domain
{
    public class Settings
	{
		[ProcessType(ProcessType.All), IsRequired(true)]
		public string Login { get; set; }

		[DisplayName("Base uri"), IsRequired(true)]
		[ProcessType(ProcessType.All)]
		public string BaseUri { get; set; }

		[DisplayName("Enter filename with processed indexes list"), IsRequired(false)]
		[ProcessType(ProcessType.CaseCreate, ProcessType.CaseSync)]
		public string ProcessedIndexesFilePath { get; set; }

		[DisplayName("Line number from which the data begins"), IsRequired(true)]
		[ProcessType(ProcessType.CaseCreate, ProcessType.CaseSync)]
		[ReadingType(ReadingMode.Excel)]
		public int DataRowPosition { get; set; }

		[DisplayName("Line number with information")]
		[ProcessType(ProcessType.CaseCreate, ProcessType.CaseSync)]
		[ReadingType(ReadingMode.Excel)] [IsRequired(true)]
		public int InformationRowPosition { get; set; }

		[DisplayName("Path to xml mapping file")]
		[ProcessType(ProcessType.CaseCreate, ProcessType.CaseSync)]
		[ReadingType(ReadingMode.XmlMap), IsRequired(true)]
		public string XmlMappingPath { get; set; }

		[DisplayName("Enter list allowed column's colors separated by commas (format:FFRRGGBB)"), IsRequired(true)]
		[ProcessType(ProcessType.CaseCreate, ProcessType.CaseSync)]
		[ReadingType(ReadingMode.Excel)]
		public string[] AllowedColors { get; set; }

		[DisplayName("Date time"), JsonIgnore, IsRequired(true)]
		[ProcessType(ProcessType.CaseDeleteByDate)]
		public string DateTime { get; set; }

		[DisplayName("Date time"), JsonIgnore, IsRequired(true)] 
		[ProcessType(ProcessType.CaseCreate), ReadingType(ReadingMode.XmlMap)]
		public string IgnoreFilePath { get; set; }

		[Ignore(true), JsonIgnore, ProcessType(ProcessType.All)]
		public Dictionary<string, dynamic> AdditionalSettings { get; set; }

		public static Settings Read(string filePath)
		{
			var info = new FileInfo(filePath);
			if (!info.Exists) File.Create(info.FullName).Dispose();

			var rawJson = File.ReadAllText(filePath);
			return string.IsNullOrEmpty(rawJson) ? new Settings() : JsonConvert.DeserializeObject<Settings>(rawJson);
		}

		public void Save(string settingsFilePath)
		{
			var jsonSettings = JsonConvert.SerializeObject(this, Formatting.Indented);

			var info = new FileInfo(settingsFilePath);

			if (!info.Exists) File.Create(info.FullName).Dispose();
			File.WriteAllText(settingsFilePath, jsonSettings);
		}
	}
}