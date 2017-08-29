using System.Collections.Generic;
using System.Linq;
using PravoAdder.Domain;
using PravoAdder.Domain.Info;

namespace PravoAdder.Readers
{
	public abstract class BlockInfoReader
	{
		public readonly string FilePath;
		public readonly ExcelTable ExcelTable;

		protected BlockInfoReader(Settings settings, ExcelTable excelInfo)
		{
			FilePath = settings.IdComparerPath;
			ExcelTable = excelInfo;
		}

		public abstract IEnumerable<BlockInfo> Read();

		public abstract HeaderBlockInfo ReadHeaderBlock(IDictionary<int, string> excelRow);

		public BlockInfo GetByName(IEnumerable<BlockInfo> blocks, string name)
		{
			return blocks
				.FirstOrDefault(block => block.Name == name);
		}
	}
}
