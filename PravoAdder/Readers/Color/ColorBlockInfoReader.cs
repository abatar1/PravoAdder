using System;
using System.Collections.Generic;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Domain;
using PravoAdder.Domain.Info;

namespace PravoAdder.Readers
{
	public class ColorBlockInfoReader : BlockInfoReader
	{
		private readonly DatabaseGetter _databaseGetter;
		private readonly Settings _settings;
		private readonly ExcelTable _excelTable;

		public ColorBlockInfoReader(ExcelTable excelTable, HttpAuthenticator authenticator, Settings settings) : base(settings.ExcelFileName, excelTable)
		{
			_databaseGetter = new DatabaseGetter(authenticator);
			_settings = settings;
			_excelTable = excelTable;
		}

		private BlockFieldInfo ReadField(dynamic projectField, FieldAddress fieldAddress)
		{
			var blockfieldInfo = new BlockFieldInfo
			{
				Id = projectField.Id,
				Name = projectField.Name,
				ColumnNumber = _excelTable.GetIndex(fieldAddress)
			};

			var fieldType = projectField.ProjectFieldFormat.SysName.ToString();
			switch (fieldType)
			{
				case "Dictionary":
					blockfieldInfo.Type = projectField.ProjectFieldFormat.SysName;
					blockfieldInfo.SpecialData = projectField.ProjectFieldFormat.Dictionary.SystemName;
					break;
				case "TextArea":
				case "Text":
				case "Date":
				case "Number":
					blockfieldInfo.Type = "Value";
					break;
				case "Participant":
					blockfieldInfo.Type = fieldType;
					break;
				default:
					throw new ArgumentException("Field type doesn't supported.");
			}

			return blockfieldInfo;
		}

		public override IEnumerable<BlockInfo> Read()
		{
			var projectType = _databaseGetter.GetProjectType(_settings.ProjectTypeName);
			var visualBlocks = _databaseGetter.GetVisualBlocks(projectType.Id.ToString());
			foreach (var visualBlock in visualBlocks)
			{
				var blockname = visualBlock.Name;
				var lines = new List<BlockLineInfo>();
				foreach (var line in visualBlock.Lines)
				{
					var fields = new List<BlockFieldInfo>();
					foreach (var field in line.Fields)
					{
						var projectField = field.ProjectField;
						var fieldAddress = new FieldAddress(blockname, projectField.Name);
						if (!_excelTable.Contains(fieldAddress)) continue;

						var blockfieldInfo = ReadField(projectField, fieldAddress);
						fields.Add(blockfieldInfo);
					}
					lines.Add(new BlockLineInfo
					{
						Id = line.Id,
						Order = 0,
						Fields = fields
					});
				}
				yield return new BlockInfo
				{
					Name = blockname,
					Id = visualBlock.Id,
					Lines = lines
				};
			}
		}

		public override HeaderBlockInfo ReadHeaderBlock(IDictionary<int, string> excelRow)
		{
			var descriptionIndex = _excelTable.GetIndex(new FieldAddress("Системный", "Описание"));
			var projectNameIndex = _excelTable.GetIndex(new FieldAddress("Системный", "Название дела"));
			var projectGroupIndex = _excelTable.GetIndex(new FieldAddress("Системный", "Проект"));
			var responsibleIndex = _excelTable.GetIndex(new FieldAddress("Системный", "Ответственный"));

			return new HeaderBlockInfo
			{
				Description = excelRow[descriptionIndex],
				ProjectGroupName = excelRow[projectGroupIndex],
				ProjectName = excelRow[projectNameIndex],
				ResponsibleName = excelRow[responsibleIndex]
			};
		}
	}
}
