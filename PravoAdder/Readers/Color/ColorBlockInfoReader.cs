using System;
using System.Collections.Generic;
using System.Linq;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Domain;
using PravoAdder.Domain.Info;

namespace PravoAdder.Readers
{
	public class ColorBlockInfoReader : BlockInfoReader
	{
		private readonly DatabaseGetter _databaseGetter;
		private readonly Settings _settings;

		public ColorBlockInfoReader(ExcelTable excelTable, Settings settings, HttpAuthenticator authenticator) : base(settings, excelTable)
		{
			_databaseGetter = new DatabaseGetter(authenticator);
			_settings = settings;
		}

		private static BlockFieldInfo ReadField(string id, dynamic projectField, int index)
		{
			var blockfieldInfo = new BlockFieldInfo
			{
				Id = id,
				Name = projectField.Name,
				ColumnNumber = index
			};

			var fieldType = projectField.ProjectFieldFormat.SysName.ToString();
			switch (fieldType)
			{
				case "Dictionary":
					blockfieldInfo.Type = projectField.ProjectFieldFormat.SysName;
					blockfieldInfo.SpecialData = projectField.ProjectFieldFormat.Dictionary.SystemName;
					break;
				case "Text":
				case "Date":
				case "TextArea":
					blockfieldInfo.Type = "Text";
					break;
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
					var tmpLines = new List<BlockLineInfo>();
					var complexMultilines = new Dictionary<int, BlockLineInfo>();
					foreach (var field in line.Fields)
					{
						var projectField = field.ProjectField;
						var fieldAddress = new FieldAddress(blockname.ToString(), projectField.Name.ToString());

						if (ExcelTable.IsComplexRepeat(fieldAddress))
						{
							var complexIndexes = ExcelTable.GetComplexIndexes(fieldAddress);
							foreach (var key in complexIndexes.Keys)
							{
								if (!complexMultilines.ContainsKey(key))
								{
									complexMultilines.Add(key, new BlockLineInfo { Id = line.Id, Order = key - 1});
								}
								complexMultilines[key].Fields.Add(ReadField(field.Id.ToString(), projectField, complexIndexes[key]));
							}
							continue;
						}						

						var indexes = ExcelTable.GetIndexes(fieldAddress);						
						if (indexes == null) continue;

						if (indexes.Count > 1)
						{
							tmpLines = indexes
								.Select(i => new BlockLineInfo
								{
									Id = line.Id,
									Order = 0,
									Fields = new List<BlockFieldInfo>
									{
										ReadField(field.Id.ToString(), projectField, i)
									}
								})
								.ToList();						
						}
						else
						{
							tmpLines.Add(new BlockLineInfo
							{
								Id = line.Id,
								Order = 0,
								Fields = indexes
									.Select(index => ReadField(field.Id.ToString(), projectField, index))
									.Cast<BlockFieldInfo>()
									.ToList()
							});
						}					
					}
					tmpLines.AddRange(complexMultilines.Select(d => d.Value));					
					lines.AddRange(tmpLines);					
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
			const string systemName = "Системный";
			var descriptionIndex = ExcelTable.TryGetIndex(new FieldAddress(systemName, "Описание"));
			var projectNameIndex = ExcelTable.TryGetIndex(new FieldAddress(systemName, "Название дела"));
			var projectGroupIndex = ExcelTable.TryGetIndex(new FieldAddress(systemName, "Название проекта"));
			var responsibleIndex = ExcelTable.TryGetIndex(new FieldAddress(systemName, "Ответственный"));
			var folderIndex = ExcelTable.TryGetIndex(new FieldAddress(systemName, "Название папки"));

			return new HeaderBlockInfo
			{
				Description = excelRow[descriptionIndex],
				ProjectGroupName = excelRow[projectGroupIndex],
				ProjectName = excelRow[projectNameIndex],
				ResponsibleName = excelRow[responsibleIndex],
				FolderName = excelRow[folderIndex]
			};
		}
	}
}
