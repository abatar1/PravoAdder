using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using PravoAdder.Domain;

namespace PravoAdder
{
    public class BlockReader
    {
        public static IEnumerable<Block> Read(string filePath)
        {
            var rawJson = File.ReadAllText(filePath);

            var blockObjects = AllChildren(JObject.Parse(rawJson))
                .First(c => c.Type == JTokenType.Array && c.Path.Contains("Blocks"))
                .Children<JObject>();

            foreach (var blockObject in blockObjects)
            {
                yield return new Block
                {
                    Id = (string)blockObject["VisualBlockId"],
                    Name = (string)blockObject["Name"],
                    Lines = blockObject["Lines"]
                        .Select(lineObject => new BlockLine
                        {
                            Id = (string)lineObject["BlockLineId"],
                            Fields = lineObject["Fields"]
                                .Select(fieldObject => new BlockField
                                {
                                    Id = (string)fieldObject["VisualBlockProjectFieldId"],
                                    ColumnNumber = (int)fieldObject["ColumnNumber"],
                                    Name = (string)fieldObject["Name"]
                                })
                        })
                };
            }
        }

        public static Block GetBlockByName(IEnumerable<Block> blocks, string name)
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
