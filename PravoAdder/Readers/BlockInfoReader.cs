using System.Collections.Generic;
using System.Linq;
using PravoAdder.Domain;
using PravoAdder.Domain.Info;

namespace PravoAdder.Readers
{
    public abstract class BlockInfoReader
    {
        public readonly Table Table;
        public readonly Settings Settings;
	    public HeaderBlockInfo HeaderBlockInfo;

        protected BlockInfoReader(Settings settings, Table excelInfo)
        {
            Settings = settings;
            Table = excelInfo;
        }

        public abstract IEnumerable<CaseInfo> Read();

        public abstract HeaderBlockInfo ReadHeaderBlock(IDictionary<int, string> excelRow);

        public BlockInfo GetByName(IEnumerable<BlockInfo> blocks, string name)
        {
            return blocks
                .FirstOrDefault(block => block.Name == name);
        }
    }
}