using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Domain;
using PravoAdder.Helpers;

namespace PravoAdder.Wrappers
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

	    public IEnumerable<VisualBlockWrapper> Build(ProjectType projectType)
	    {
		    if (HeaderBlockInfo.ProjectType == null && projectType == null) return null;

		    if (projectType == null)
		    {
				projectType = ApiEnviroment.GetProjectType(_httpAuthenticator, HeaderBlockInfo, 0);
			    if (projectType == null) return null;
			}		   		    

			var visualBlocks = GetVisualBlocks(projectType.Id);

		    var result = new Dictionary<int, List<VisualBlock>>();
		    foreach (var block in visualBlocks)
		    {					
			    var clonedBlock = block.CloneJson();
			    var correctBlockName = clonedBlock.Name.Split('-')[0].Trim();

				var blockNumbers = clonedBlock.IsRepeatable
				    ? Table.GetRepeatBlockNumber(correctBlockName)
				    : new List<int> {0};
			    foreach (var blockNumber in blockNumbers)
			    {
				    var newLines = new List<VisualBlockLine>();
				    foreach (var line in clonedBlock.Lines)
				    {
					    var clonedLine = line.CloneJson();

					    var simpleRepeatsLines = new List<VisualBlockLine>();
					    var complexMultilines = new Dictionary<int, VisualBlockLine>();

					    foreach (var field in clonedLine.Fields)
					    {						   
						    var fieldAddress = new FieldAddress(correctBlockName, field.ProjectField.Name);
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
								    var multiField = field.CloneJson();
								    multiField.ColumnNumber = complexIndexes[key];
								    complexMultilines[key].Fields.Add(multiField);
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
									    var multiField = field.CloneJson();
									    multiField.ColumnNumber = i;
									    return new VisualBlockLine
									    {
										    BlockLineId = clonedLine.Id,
										    Order = 0,
										    Fields = new List<VisualBlockField> { multiField }
									    };
								    })
								    .ToList();
								continue;
						    }
						    if (clonedLine.IsSimple)
						    {
							    var clonedField = field.CloneJson();
								if (simpleRepeatsLines.Count == 0) simpleRepeatsLines.Add(new VisualBlockLine(clonedLine.Id, 0));
							    clonedField.ColumnNumber = indexes.First();
								simpleRepeatsLines[0].Fields.Add(clonedField);
							}
					    }
					    newLines.AddRange(simpleRepeatsLines);
					    newLines.AddRange(complexMultilines.Select(d => d.Value));
				    }
				    clonedBlock.Lines = new List<VisualBlockLine>(newLines);
				    if (!result.ContainsKey(blockNumber)) result.Add(blockNumber, new List<VisualBlock>());
				    result[blockNumber].Add(clonedBlock);
			    }			  
		    }
		    return result
				.Select(x => new VisualBlockWrapper {VisualBlocks = x.Value.Where(y => y.Lines.Count > 0).ToList(), Order = x.Key});
	    }

	    public HeaderBlockInfo ReadHeaderBlock(Row excelRow)
	    {
		    var headerObject = new HeaderBlockInfo();
		    foreach (var property in headerObject.GetType().GetProperties())
		    {
			    var fieldNameAttibute = (FieldNameAttribute) property
				    .GetCustomAttributes(typeof(FieldNameAttribute), true)
				    .FirstOrDefault();
			    if (fieldNameAttibute == null) continue;

			    var langNames = HeaderBlockInfo.SystemNames.Select(k => k.Value).Zip(fieldNameAttibute.FieldNames,
				    (block, field) => new { Block = block, Field = field });
			    foreach (var name in langNames)
			    {
				    var index = Table.TryGetIndex(new FieldAddress(name.Block, name.Field));

					if (!excelRow.Content.ContainsKey(index)) continue;
				    if (index == 0) continue;

				    property.SetValue(headerObject, excelRow[index].ToString().Trim());
				    break;
			    }
		    }
		    HeaderBlockInfo = headerObject;
		    return headerObject;
	    }
	}
}