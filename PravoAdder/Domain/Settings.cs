using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		[ProcessType(ProcessType.Migration, ProcessType.Sync)]
		public string ProcessedIndexesFilePath { get; set; }

		[DisplayName("Line number from which the data begins"), IsRequired(true)]
		[ProcessType(ProcessType.Migration, ProcessType.Sync)]
		[ReadingType(ReaderMode.Excel)]
		public int DataRowPosition { get; set; }

		[DisplayName("Line number with information")]
		[ProcessType(ProcessType.Migration, ProcessType.Sync)]
		[ReadingType(ReaderMode.Excel)] [IsRequired(true)]
		public int InformationRowPosition { get; set; }

		[DisplayName("Path to xml mapping file")]
		[ProcessType(ProcessType.Migration, ProcessType.Sync)]
		[ReadingType(ReaderMode.XmlMap), IsRequired(true)]
		public string XmlMappingPath { get; set; }

		[DisplayName("Enter list allowed column's colors separated by commas (format:FFRRGGBB)"), IsRequired(true)]
		[ProcessType(ProcessType.Migration, ProcessType.Sync)]
		[ReadingType(ReaderMode.Excel)]
		public string[] AllowedColors { get; set; }

		[DisplayName("Date time"), JsonIgnore, IsRequired(true)]
		[ProcessType(ProcessType.CleanByDate)]
		public string DateTime { get; set; }

		[DisplayName("Date time"), JsonIgnore, IsRequired(true)] 
		[ProcessType(ProcessType.Migration), ReadingType(ReaderMode.XmlMap)]
		public string IgnoreFilePath { get; set; }

		[Ignore(true), JsonIgnore, ProcessType(ProcessType.All)]
		public Dictionary<string, dynamic> AdditionalSettings { get; set; }
	}
}