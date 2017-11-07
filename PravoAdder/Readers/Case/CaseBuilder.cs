using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;
using PravoAdder.Domain;
using PravoAdder.Domain.Attributes;
using PravoAdder.Helpers;
using PravoAdder.Wrappers;

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

	    public IEnumerable<VisualBlockWrapper> Build()
	    {
		    if (HeaderBlockInfo.ProjectType == null) return null;
	   
		    var projectType = ApiEnviroment.GetProjectType(_httpAuthenticator, HeaderBlockInfo, 0);
			var visualBlocks = GetVisualBlocks(projectType.Id);

		    var result = new Dictionary<int, List<VisualBlock>>();
		    foreach (var block in visualBlocks)
		    {
			    var correctBlockName = block.Name.Split('-')[0].Trim();
				var blockNumbers = block.IsRepeatable
				    ? Table.GetRepeatBlockNumber(correctBlockName)
				    : new List<int> {0};
			    foreach (var blockNumber in blockNumbers)
			    {
				    var newLines = new List<VisualBlockLine>();
				    foreach (var line in block.Lines)
				    {
					    var clonedLine = line.CloneJson();

					    var simpleRepeatsLines = new List<VisualBlockLine>();
					    var complexMultilines = new Dictionary<int, VisualBlockLine>();

					    foreach (var field in clonedLine.Fields)
					    {
						    var clonedField = field.CloneJson();

						    var fieldAddress = new FieldAddress(correctBlockName, clonedField.ProjectField.Name);
						    var fieldCount = clonedLine.Fields.Count;

						    if (clonedLine.IsRepeated && fieldCount > 1)
						    {
							    var complexIndexes = Table.GetComplexIndexes(fieldAddress, blockNumber);
							    foreach (var key in complexIndexes.Keys)
							    {
								    if (!complexMultilines.ContainsKey(key))
								    {
									    complexMultilines.Add(key, new VisualBlockLine(clonedLine.Id, key - 1));
								    }
								    clonedField.ColumnNumber = complexIndexes[key];
								    complexMultilines[key].Fields.Add(clonedField);
							    }
							    continue;
						    }

						    var indexes = Table.GetIndexes(fieldAddress, blockNumber);
						    if (indexes == null || indexes.Count == 0) continue;
						    if (clonedLine.IsRepeated && fieldCount == 1)
						    {
							    simpleRepeatsLines = indexes
								    .Select(i =>
								    {
									    clonedField.ColumnNumber = i;
									    return new VisualBlockLine
									    {
										    BlockLineId = clonedLine.Id,
										    Order = 0,
										    Fields = new List<VisualBlockField> {clonedField}
									    };
								    })
								    .ToList();
								continue;
						    }
						    if (clonedLine.IsSimple)
						    {
							    if (simpleRepeatsLines.Count == 0) simpleRepeatsLines.Add(new VisualBlockLine(clonedLine.Id, 0));
							    clonedField.ColumnNumber = indexes.First();
								simpleRepeatsLines[0].Fields.Add(clonedField);
							}
					    }
					    newLines.AddRange(simpleRepeatsLines);
					    newLines.AddRange(complexMultilines.Select(d => d.Value.CloneJson()));
				    }
				    block.Lines = new List<VisualBlockLine>(newLines);
				    if (!result.ContainsKey(blockNumber)) result.Add(blockNumber, new List<VisualBlock>());
				    result[blockNumber].Add(block);
			    }			  
		    }
		    return result
				.Select(x => new VisualBlockWrapper {VisualBlocks = x.Value, Order = x.Key});
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
	}
}