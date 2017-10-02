using PravoAdder.Domain;

namespace PravoAdder.TableEnviroment
{
	public class TableSettings
	{
		public string SourceFileName { get; set; }
		public string[] AllowedColors { get; set; }
		public int DataRowPosition { get; set; }
		public int InformationRowPosition { get; set; }
		public int StartRow { get; set; }
		public string XmlMappingPath { get; set; }
		public string ProcessedIndexesFilePath { get; set; }
		public ReaderMode BlockReadingMode { get; set; }

		public TableSettings(Settings settings)
		{
			SourceFileName = settings.SourceFileName;
			AllowedColors = settings.AllowedColors;
			DataRowPosition = settings.DataRowPosition;
			InformationRowPosition = settings.InformationRowPosition;
			StartRow = settings.StartRow;
			XmlMappingPath = settings.XmlMappingPath;
			ProcessedIndexesFilePath = settings.ProcessedIndexesFilePath;
			BlockReadingMode = settings.BlockReadingMode;
		}
	}
}
