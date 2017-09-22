using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;
using PravoAdder.Domain;
using PravoAdder.Domain.Info;

namespace PravoAdder.Readers.Color
{
    public class ColorBlockInfoReader : BlockInfoReader
    {
	    private readonly IDictionary<string, List<VisualBlock>> _visualBlocks;
		private readonly object _visualBlocksLocker = new object();
	    private readonly HttpAuthenticator _httpAuthenticator;

        public ColorBlockInfoReader(Table excelTable, Settings settings, HttpAuthenticator httpAuthenticator) :
            base(settings, excelTable)
        {
			_visualBlocks = new ConcurrentDictionary<string, List<VisualBlock>>();
	        _httpAuthenticator = httpAuthenticator;
        }

	    private IEnumerable<VisualBlock> GetVisualBlocks(string projectTypeId)
	    {
			List<VisualBlock> visualBlocks;
		    lock (_visualBlocksLocker)
		    {
			    if (!_visualBlocks.ContainsKey(projectTypeId))
			    {
				    var newVisualBlocks = ApiRouter.ProjectTypes.GetVisualBlocks(_httpAuthenticator, projectTypeId);
				    _visualBlocks.Add(projectTypeId, newVisualBlocks);
			    }
			    visualBlocks = _visualBlocks[projectTypeId];
		    }
		    return visualBlocks;
	    }

	    public override IEnumerable<CaseInfo> Read()
	    {
		    if (HeaderBlockInfo.ProjectTypeName == null) return null;

		    var projectType = ApiRouter.ProjectTypes.GetProjectTypes(_httpAuthenticator)
			    .GetByName(HeaderBlockInfo.ProjectTypeName);
		    var visualBlocks = GetVisualBlocks(projectType.Id);

		    var result = new Dictionary<int, List<BlockInfo>>();
		    foreach (var block in visualBlocks)
		    {
			    var blockNumbers = block.IsRepeatable
				    ? Table.GetRepeatBlockNumber(block.Name)
				    : new List<int> {0};
			    foreach (var blockNumber in blockNumbers)
			    {
				    var lines = new List<BlockLineInfo>();
				    foreach (var line in block.Lines)
				    {
					    var simpleRepeatsLines = new List<BlockLineInfo>();
					    var complexMultilines = new Dictionary<int, BlockLineInfo>();

					    foreach (var field in line.Fields)
					    {
						    var fieldAddress = new FieldAddress(block.Name, field.ProjectField.Name);
						    var fieldCount = line.Fields.Count;

						    if (line.LineType.SysName == "Repeated" && fieldCount > 1)
						    {
							    var complexIndexes = Table.GetComplexIndexes(fieldAddress, blockNumber);
							    foreach (var key in complexIndexes.Keys)
							    {
								    if (!complexMultilines.ContainsKey(key))
								    {
									    complexMultilines.Add(key, new BlockLineInfo(line.Id, key - 1));
								    }
								    complexMultilines[key].Fields
									    .Add(BlockFieldInfo.Create(field, complexIndexes[key]));
							    }
							    continue;
						    }

						    var indexes = Table.GetIndexes(fieldAddress, blockNumber);
						    if (indexes == null || indexes.Count == 0) continue;
						    if (line.LineType.SysName == "Repeated" && fieldCount == 1)
						    {
							    simpleRepeatsLines = indexes
								    .Select(i => new BlockLineInfo
								    {
									    Id = line.Id,
									    Order = 0,
									    Fields = new List<BlockFieldInfo> {BlockFieldInfo.Create(field, i)}
								    })
								    .ToList();
						    }
						    if (line.LineType.SysName == "Simple")
						    {
							    if (simpleRepeatsLines.Count == 0) simpleRepeatsLines.Add(new BlockLineInfo(line.Id, 0));
							    simpleRepeatsLines[0].Fields.Add(BlockFieldInfo.Create(field, indexes.First()));
						    }
					    }
					    lines.AddRange(simpleRepeatsLines);
					    lines.AddRange(complexMultilines.Select(d => (BlockLineInfo) d.Value.Clone()));
				    }

				    if (!result.ContainsKey(blockNumber)) result.Add(blockNumber, new List<BlockInfo>());
				    result[blockNumber].Add(new BlockInfo(block.Name, block.Id, lines));
			    }			  
		    }
		    return result
				.Select(x => new CaseInfo {Blocks = x.Value, Order = x.Key});
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

		        var index = Table.TryGetIndex(new FieldAddress(systemName, fieldnameAttibute?.FieldName));
		        if (index == 0) continue;

				property.SetValue(headerObject, excelRow[index]);
	        }
	        HeaderBlockInfo = headerObject;
	        return headerObject;
        }
    }
}