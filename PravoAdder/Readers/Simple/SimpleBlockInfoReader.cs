using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using PravoAdder.Domain;

namespace PravoAdder.Readers
{
    public class SimpleBlockInfoReader : BlockInfoReader
    {
        public SimpleBlockInfoReader(Table excelInfo, Settings settings) : base(settings, excelInfo)
        {
        }

        //public override IEnumerable<BlockInfo> Read()
        //{
        //    var info = new FileInfo(Settings.IdComparerPath);
        //    if (!info.Exists) throw new FileNotFoundException("Blocks info file doesn't found.");

        //    var jBlocks = JObject.Parse(File.ReadAllText(Settings.IdComparerPath));

        //    var blockObjects = jBlocks.GetAllChildrens()
        //        .First(c => c.Type == JTokenType.Array && c.Path.Contains("Blocks"))
        //        .Children<JObject>();

        //    foreach (var blockObject in blockObjects)
        //        yield return new BlockInfo
        //        {
        //            Id = (string) blockObject["VisualBlockId"],
        //            Name = (string) blockObject["Name"],
        //            Lines = blockObject["Lines"]
        //                .Select(lineObject => new BlockLineInfo
        //                {
        //                    Id = (string) lineObject["BlockLineId"],
        //                    Fields = lineObject["Values"]
        //                        .Select(delegate(JToken fieldObject)
        //                        {
        //                            var field = new BlockFieldInfo
        //                            {
        //                                Id = (string) fieldObject["VisualBlockProjectFieldId"],
        //                                ColumnNumber = (int) fieldObject["ColumnNumber"],
        //                                Name = (string) fieldObject["Name"],
        //                                Type = (string) fieldObject["Type"]
        //                            };
        //                            if (field.Type != "Value")
        //                                field.SpecialData = (string) fieldObject["SpecialData"];
        //                            return field;
        //                        })
        //                        .ToList()
        //                })
        //        };
        //}

	    public override IEnumerable<CaseInfo> Read()
	    {
		    throw new System.NotImplementedException();
	    }

	    public override HeaderBlockInfo ReadHeaderBlock(IDictionary<int, string> excelRow)
        {
            var jBlocks = JObject.Parse(File.ReadAllText(Settings.IdComparerPath));
            return new HeaderBlockInfo
            {
                ProjectName = excelRow[jBlocks["ProjectName"].ToObject<int>()],
                ProjectGroupName = excelRow[jBlocks["ProjectGroupName"].ToObject<int>()],
                ResponsibleName = excelRow[jBlocks["ResponsibleName"].ToObject<int>()],
                FolderName = jBlocks["ProjectFolderName"].ToObject<string>()
            };
        }
    }
}