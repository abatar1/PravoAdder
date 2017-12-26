using System;
using System.IO;
using Newtonsoft.Json;

namespace PravoAdder.Domain
{
    public class Settings
	{
		[Required]
		[DefaultValue("template name")]
        public string Name { get; set; }

		[Required]
		public string BaseUri { get; set; }

		[Required]
		public string UserName { get; set; }

		[Required]
		public string Password { get; set; }

		[Required]
		public string ProcessType { get; set; }

		[Required]
		[DefaultValue(1)]
		public int ParallelOptions { get; set; }		

		public DataReadingMode DataReadingMode { get; set; }

		public FieldReadingMode FieldReadingMode { get; set; }

		[DefaultValue(2)]
		public int DataRowPosition { get; set; }

		[DefaultValue(1)]
		public int InformationRowPosition { get; set; }

		public string XmlMappingPath { get; set; }

		[DefaultValue("FFFFFF00")]
		public string AllowedColor { get; set; }	
		
		public string SourceName { get; set; }

		public string SecondSourceName { get; set; }

		[DefaultValue(0)]
		public int RowNum { get; set; }		

		public bool IsOverwrite { get; set; }

		public bool CreateNewPracticeArea { get; set; }

		public string ParticipantType { get; set; }

		public DateTime Date { get; set; }

		public string SearchKey { get; set; }

		public string ProjectType { get; set; }

		public string Language { get; set; }

		public string FilePath;

		private const string InstancesDirectoryPath = ".\\instances";

		public Settings()
		{
			
		}

		public Settings(string instanceName)
		{
			SetFilePath(instanceName);

			var info = new FileInfo(FilePath);
			if (!info.Exists) File.Create(info.FullName).Dispose();

			var rawJson = File.ReadAllText(FilePath);

			if (!string.IsNullOrEmpty(rawJson))
			{
				var newObj = JsonConvert.DeserializeObject<Settings>(rawJson);
				foreach (var property in typeof(Settings).GetProperties())
				{
					var newValue = property.GetValue(newObj);
					property.SetValue(this, newValue);
				}
			}
		}

		public void SetFilePath(string fileName = null)
		{
			if (!Directory.Exists(InstancesDirectoryPath)) Directory.CreateDirectory(InstancesDirectoryPath);
			FilePath = $"{InstancesDirectoryPath}\\{fileName ?? Name}.json";
		}

		public bool Save()
		{
			var creationStatus = false;

			if (string.IsNullOrEmpty(FilePath)) throw new ArgumentException("You need to SetFilePath() before Save().");

			var jsonSettings = JsonConvert.SerializeObject(this, Formatting.Indented);

			var info = new FileInfo(FilePath);

			if (!info.Exists)
			{
				File.Create(info.FullName).Dispose();
				creationStatus = true;
			}

			File.WriteAllText(FilePath, jsonSettings);

			return creationStatus;
		}
	}
}