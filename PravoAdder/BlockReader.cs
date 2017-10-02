using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;
using PravoAdder.Domain;
using PravoAdder.TableEnviroment;

namespace PravoAdder
{
    public class BlockReader
    {
	    private readonly IDictionary<string, List<VisualBlock>> _visualBlocks = new ConcurrentDictionary<string, List<VisualBlock>>();
		private readonly object _visualBlocksLocker = new object();
	    private readonly Table _mainTable = TablesContainer.Table;
	    private readonly HttpAuthenticator _httpAuthenticator;
	    private HeaderBlockInfo _headerBlockInfo;

		public BlockReader(HttpAuthenticator httpAuthenticator)
        {		
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

	    public IList<CaseInfo> Read()
	    {
		    if (_headerBlockInfo.ProjectTypeName == null) return null;

		    var projectType = ApiRouter.ProjectTypes.GetProjectTypes(_httpAuthenticator)
			    .GetByName(_headerBlockInfo.ProjectTypeName);
		    var visualBlocks = GetVisualBlocks(projectType.Id);

		    var result = new Dictionary<int, List<BlockInfo>>();
		    foreach (var block in visualBlocks)
		    {
			    var blockNumbers = block.IsRepeatable
				    ? _mainTable.GetRepeatBlockNumber(block.Name)
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
						    var fieldCount = line.Fields.Count;
						    var fieldAddress = _mainTable.GetFullAddressInfo(block.Name, field.ProjectField.Name);
							if (fieldAddress == null) continue;
						    var reference = fieldAddress.IsReference ? fieldAddress.Reference : null;

						    if (line.LineType.SysName == "Repeated" && fieldCount > 1)
						    {
							    var complexIndexes = _mainTable.GetComplexIndexes(fieldAddress, blockNumber);
							    foreach (var key in complexIndexes.Keys)
							    {
								    if (!complexMultilines.ContainsKey(key))
								    {
									    complexMultilines.Add(key, new BlockLineInfo(line.Id, key - 1));
								    }
								    complexMultilines[key].Fields
									    .Add(BlockFieldInfo.Create(field, complexIndexes[key], reference));
							    }
							    continue;
						    }

						    var indexes = _mainTable.GetIndexes(fieldAddress, blockNumber);
						    if (indexes == null || indexes.Count == 0) continue;
						    if (line.LineType.SysName == "Repeated" && fieldCount == 1)
						    {
							    simpleRepeatsLines = indexes
								    .Select(i => new BlockLineInfo
								    {
									    Id = line.Id,
									    Order = 0,
									    Fields = new List<BlockFieldInfo> {BlockFieldInfo.Create(field, i, reference)}
								    })
								    .ToList();
						    }
						    if (line.LineType.SysName == "Simple")
						    {
							    if (simpleRepeatsLines.Count == 0) simpleRepeatsLines.Add(new BlockLineInfo(line.Id, 0));
							    simpleRepeatsLines[0].Fields.Add(BlockFieldInfo.Create(field, indexes.First(), reference));
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
				.Select(x => new CaseInfo {Blocks = x.Value, Order = x.Key})
				.ToList();
	    }

        public HeaderBlockInfo ReadHeaderBlock(Row excelRow)
        {
            var systemNames = new [] { "Системный", "Summary"};
	        var headerObject = new HeaderBlockInfo();
	        foreach (var property in headerObject.GetType().GetProperties())
	        {
		        var fieldnameAttibute = (FieldNameAttribute) property
			        .GetCustomAttributes(typeof(FieldNameAttribute), true)
			        .FirstOrDefault();
		        if (fieldnameAttibute == null) continue;

		        var langNames = systemNames.Zip(fieldnameAttibute.FieldNames,
			        (block, field) => new {Block = block, Field = field});
		        foreach (var name in langNames)
		        {
			        var index = _mainTable.TryGetIndex(new FieldInfo(name.Block, name.Field));
			        if (index == 0) continue;
			        property.SetValue(headerObject, excelRow[index].Value);
			        break;
		        }
	        }
	        if (string.IsNullOrEmpty(headerObject.ProjectName) || string.IsNullOrEmpty(headerObject.ProjectTypeName)) return null;

			_headerBlockInfo = headerObject;
	        return headerObject;
        }

	    public BlockInfo GetByName(IEnumerable<BlockInfo> blocks, string name)
	    {
		    return blocks
			    .FirstOrDefault(block => block.Name == name);
	    }
	}
}