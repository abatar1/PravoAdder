using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

namespace PravoAdder.Domain
{
    public class Settings
	{
		[DisplayName("Line number from which the data begins"), IsRequired(true)]
		[ReadingType(ReadingMode.Excel)]
		public int DataRowPosition { get; set; }

		[DisplayName("Line number with information")]
		[ReadingType(ReadingMode.Excel)] [IsRequired(true)]
		public int InformationRowPosition { get; set; }

		[DisplayName("Path to xml mapping file")]
		[ReadingType(ReadingMode.XmlMap), IsRequired(true)]
		public string XmlMappingPath { get; set; }

		[DisplayName("Enter list allowed column's colors separated by commas (format:FFRRGGBB)"), IsRequired(true)]
		[ReadingType(ReadingMode.Excel)]
		public string[] AllowedColors { get; set; }

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