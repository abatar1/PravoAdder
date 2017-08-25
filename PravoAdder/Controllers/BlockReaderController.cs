using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Domain;
using PravoAdder.Domain.Info;
using PravoAdder.Readers;

namespace PravoAdder.Controllers
{
	public class BlockReaderController
	{
		private readonly BlockInfoReader _blockInfoReader;
		public ExcelTable ExcelTable { get; private set; }

		public BlockReaderController(TextWriter writer, Settings settings, HttpAuthenticator autentificator)
		{
			
			Console.SetOut(writer);
			ExcelReader excelReader;
			switch (settings.BlockLoadingMode)
			{
				case "Color":
					excelReader = new ColorExcelReader();
					ExcelTable = excelReader.Read(settings);
					_blockInfoReader = new ColorBlockInfoReader(ExcelTable, autentificator, settings);					
					break;
				case "Simple":
					excelReader = new SimpleExcelReader();
					ExcelTable = excelReader.Read(settings);
					_blockInfoReader = new SimpleBlockInfoReader(settings.IdComparerPath, ExcelTable);
					break;
				default:
					throw new NotImplementedException();
			}
		}

		public IList<BlockInfo> ReadBlockInfo()
		{
			return _blockInfoReader.Read().ToList();
		}

		public HeaderBlockInfo ReadHeader(IDictionary<int, string> excelRow)
		{
			return _blockInfoReader.ReadHeaderBlock(excelRow);
		}
	}
}
