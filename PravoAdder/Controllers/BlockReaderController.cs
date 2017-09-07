using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Domain;
using PravoAdder.Domain.Info;
using PravoAdder.Readers;

namespace PravoAdder.Controllers
{
    public class BlockReaderController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly BlockInfoReader _blockInfoReader;

        public BlockReaderController(Settings settings, HttpAuthenticator autentificator)
        {
            ExcelReader excelReader;
            switch (settings.BlockLoadingMode)
            {
                case "Color":
                    excelReader = new ColorExcelReader();
                    ExcelTable = excelReader.Read(settings);
                    _blockInfoReader = new ColorBlockInfoReader(ExcelTable, settings, autentificator);
                    break;
                case "Simple":
                    excelReader = new SimpleExcelReader();
                    ExcelTable = excelReader.Read(settings);
                    _blockInfoReader = new SimpleBlockInfoReader(ExcelTable, settings);
                    break;
                default:
                    var message = $"Типа блоков {settings.BlockLoadingMode} не существует.";
                    Logger.Error(message);
                    throw new ArgumentException(message);
            }
        }

        public ExcelTable ExcelTable { get; }

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