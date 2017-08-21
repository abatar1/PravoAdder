using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using PravoAdder.Domain.Info;

namespace PravoAdder.Reader
{
    public class BlockInfoReader
    {
        public static IEnumerable<BlockInfo> Read(string filePath)
        {
            var info = new FileInfo(filePath);
            if (!info.Exists) throw new FileNotFoundException("Blocks info file doesn't found.");
            var jBlocks = JObject.Parse(File.ReadAllText(filePath));

            var blockObjects = AllChildren(jBlocks)
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("Blocks"))
                .Children<JObject>();

            foreach (var blockObject in blockObjects)
            {
                yield return new BlockInfo
                {
                    Id = (string)blockObject["VisualBlockId"],
                    Name = (string)blockObject["Name"],
                    Lines = blockObject["Lines"]
                        .Select(lineObject => new BlockLineInfo
                        {
                            Id = (string)lineObject["BlockLineId"],
                            Fields = lineObject["Values"]
                                .Select(delegate(JToken fieldObject)
                                {
                                    var field = new BlockFieldInfo
                                    {
                                        Id = (string) fieldObject["VisualBlockProjectFieldId"],
                                        ColumnNumber = (int) fieldObject["ColumnNumber"],
                                        Name = (string) fieldObject["Name"],
                                        Type = (string) fieldObject["Type"]
                                    };
                                    if (field.Type != "Value")
                                        field.SpecialData = (string)fieldObject["SpecialData"];
                                    return field;
                                })
                        })
                };
            }
        }

        public static HeaderBlockInfo ReadHeader(string filePath, IDictionary<int, string> excelRow)
        {
            var jBlocks = JObject.Parse(File.ReadAllText(filePath));

            return new HeaderBlockInfo
            {
                ProjectName = excelRow[jBlocks["ProjectName"].ToObject<int>()],
                ProjectGroupName = excelRow[jBlocks["ProjectGroupName"].ToObject<int>()],
                ResponsibleName = excelRow[jBlocks["ResponsibleName"].ToObject<int>()]
            };
        }

        public static BlockInfo GetByName(IEnumerable<BlockInfo> blocks, string name)
        {
            return blocks
                .FirstOrDefault(block => block.Name == name);
        }

        private static IEnumerable<JToken> AllChildren(JToken json)
        {
            foreach (var c in json.Children())
            {
                yield return c;
                foreach (var cc in AllChildren(c))
                {
                    yield return cc;
                }
            }
        }
    }
}
