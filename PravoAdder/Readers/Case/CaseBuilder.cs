using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;
using PravoAdder.Domain;
using PravoAdder.Domain.Attributes;

namespace PravoAdder.Readers
{
    public class CaseBuilder
    {
	    private readonly IDictionary<string, List<VisualBlock>> _visualBlocks;
		private readonly object _visualBlocksLocker = new object();
	    private readonly HttpAuthenticator _httpAuthenticator;

		public readonly Table Table;
	    public readonly Settings Settings;
	    public HeaderBlockInfo HeaderBlockInfo;    

		public CaseBuilder(Table excelTable, Settings settings, HttpAuthenticator httpAuthenticator)
        {
	        Settings = settings;
	        Table = excelTable;
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

	    public IEnumerable<CaseInfo> Build()
	    {
		    if (HeaderBlockInfo.ProjectType == null) return null;

		    var projectType = ApiRouter.ProjectTypes.GetProjectTypes(_httpAuthenticator)
			    .GetByName(HeaderBlockInfo.ProjectType);
		    var visualBlocks = GetVisualBlocks(projectType.Id);

		    var result = new Dictionary<int, List<BlockInfo>>();
		    foreach (var block in visualBlocks)
		    {
			    var correctBlockName = block.Name.Split('-')[0].Trim();
				var blockNumbers = block.IsRepeatable
				    ? Table.GetRepeatBlockNumber(correctBlockName)
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
						    var fieldAddress = new FieldAddress(correctBlockName, field.ProjectField.Name);
						    var fieldCount = line.Fields.Count;

						    if (line.IsRepeated && fieldCount > 1)
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
						    if (line.IsRepeated && fieldCount == 1)
						    {
							    simpleRepeatsLines = indexes
								    .Select(i => new BlockLineInfo
								    {
									    BlockLineId = line.Id,
									    Order = 0,
									    Fields = new List<BlockFieldInfo> {BlockFieldInfo.Create(field, i)}
								    })
								    .ToList();
						    }
						    if (line.IsSimple)
						    {
							    if (simpleRepeatsLines.Count == 0) simpleRepeatsLines.Add(new BlockLineInfo(line.Id, 0));
							    simpleRepeatsLines[0].Fields.Add(BlockFieldInfo.Create(field, indexes.First()));
						    }
					    }
					    lines.AddRange(simpleRepeatsLines);
					    lines.AddRange(complexMultilines.Select(d => (BlockLineInfo) d.Value.Clone()));
				    }

				    if (!result.ContainsKey(blockNumber)) result.Add(blockNumber, new List<BlockInfo>());
				    result[blockNumber].Add(new BlockInfo(correctBlockName, block.Id, lines));
			    }			  
		    }
		    return result
				.Select(x => new CaseInfo {Blocks = x.Value, Order = x.Key});
	    }

	    public HeaderBlockInfo ReadHeaderBlock(Row excelRow)
	    {
		    var headerObject = new HeaderBlockInfo();
		    foreach (var property in headerObject.GetType().GetProperties())
		    {
			    var fieldnameAttibute = (FieldNameAttribute) property
				    .GetCustomAttributes(typeof(FieldNameAttribute), true)
				    .FirstOrDefault();
			    if (fieldnameAttibute == null) continue;

			    var langNames = HeaderBlockInfo.SystemNames.Select(k => k.Value).Zip(fieldnameAttibute.FieldNames,
				    (block, field) => new { Block = block, Field = field });
			    foreach (var name in langNames)
			    {
				    var index = Table.TryGetIndex(new FieldAddress(name.Block, name.Field));
				    if (index == 0) continue;

				    property.SetValue(headerObject, excelRow[index].ToString().Trim());
				    break;
			    }
		    }
		    if (string.IsNullOrEmpty(headerObject.Name) || string.IsNullOrEmpty(headerObject.ProjectType)) return null;
		    HeaderBlockInfo = headerObject;
		    return headerObject;
	    }

		public BlockInfo GetByName(IEnumerable<BlockInfo> blocks, string name)
	    {
		    return blocks
			    .FirstOrDefault(block => block.Name == name);
	    }
	}
}