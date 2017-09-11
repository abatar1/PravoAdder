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

        public ColorBlockInfoReader(ExcelTable excelTable, Settings settings, HttpAuthenticator authenticator) :
            base(settings, excelTable)
        {
            _databaseGetter = new DatabaseGetter(authenticator);
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
            var projectType = _databaseGetter.GetProjectType(HeaderBlockInfo.ProjectTypeName);
            var visualBlocks = _databaseGetter.GetVisualBlocks(projectType.Id.ToString());
            foreach (var visualBlock in visualBlocks)
            {
                var blockname = visualBlock.Name;
                var lines = new List<BlockLineInfo>();
                foreach (var line in visualBlock.Lines)
                {
                    var lineId = line.Id.ToString();
                    var simpleRepeatsLines = new List<BlockLineInfo>();
                    var simpleLine = new BlockLineInfo(lineId, 0);
                    var complexMultilines = new Dictionary<int, BlockLineInfo>();

                    foreach (var field in line.Fields)
                    {
                        var projectField = field.ProjectField;
                        var fieldId = field.Id.ToString();
                        var fieldAddress = new FieldAddress(blockname.ToString(), projectField.Name.ToString());

                        if (ExcelTable.IsComplexRepeat(fieldAddress))
                        {
                            var complexIndexes = ExcelTable.GetComplexIndexes(fieldAddress);
                            foreach (var key in complexIndexes.Keys)
                            {
                                if (!complexMultilines.ContainsKey(key))
                                    complexMultilines.Add(key, new BlockLineInfo(lineId, key - 1));
                                complexMultilines[key].Fields
                                    .Add(ReadField(fieldId, projectField, complexIndexes[key]));
                            }
                            continue;
                        }

                        var indexes = ExcelTable.GetIndexes(fieldAddress);
                        if (indexes == null) continue;

                        if (indexes.Count > 1)
                            simpleRepeatsLines = indexes
                                .Select(i => new BlockLineInfo
                                {
                                    Id = lineId,
                                    Order = 0,
                                    Fields = new List<BlockFieldInfo>
                                    {
                                        ReadField(fieldId, projectField, i)
                                    }
                                })
                                .ToList();
                        else
                            simpleLine.Fields.Add(ReadField(fieldId, projectField, indexes.First()));
                    }

                    lines.AddRange(simpleRepeatsLines);
                    lines.AddRange(complexMultilines.Select(d => (BlockLineInfo) d.Value.Clone()));
                    lines.Add(simpleLine);
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
	        var headerObject = new HeaderBlockInfo();
	        foreach (var property in headerObject.GetType().GetProperties())
	        {
		        var fieldnameAttibute = (FieldNameAttribute) property
			        .GetCustomAttributes(typeof(FieldNameAttribute), true)
			        .FirstOrDefault();

		        var index = ExcelTable.TryGetIndex(new FieldAddress(systemName, fieldnameAttibute?.FieldName));
		        if (index == 0) continue;

				property.SetValue(headerObject, excelRow[index]);
	        }
	        HeaderBlockInfo = headerObject;
	        return headerObject;
        }
    }
}