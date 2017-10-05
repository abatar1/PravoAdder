using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using PravoAdder.Api;
using PravoAdder.Domain;
using PravoAdder.Readers;

namespace PravoAdder.Wrappers
{
    public class BlockReaderWrapper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly BlocksConstructor _blockInfoReader;

        public BlockReaderWrapper(Settings settings, HttpAuthenticator autentificator)
        {
            TableReader tableReader;
            switch (settings.BlockReadingMode)
            {
                case ReaderMode.Excel:
                    tableReader = new ExcelReader();
                    Table = tableReader.Read(settings);
                    break;
				case ReaderMode.XmlMap:
					tableReader = new XmlWithMappingReader();
					Table = tableReader.Read(settings);
					_blockInfoReader = new BlocksConstructor(Table, settings, autentificator);
					break;
                default:
                    var message = $"Типа блоков {settings.BlockReadingMode} не существует.";
                    Logger.Error(message);
                    throw new ArgumentException(message);
            }
			_blockInfoReader = new BlocksConstructor(Table, settings, autentificator);
		}

        public Table Table { get; }

        public IList<CaseInfo> ReadBlockInfo()
        {
            return _blockInfoReader.Read().ToList();
        }

        public HeaderBlockInfo ReadHeader(IDictionary<int, string> tableRow)
        {
            var headerBlock = _blockInfoReader.ReadHeaderBlock(tableRow);
	        if (string.IsNullOrEmpty(headerBlock.ProjectName) || string.IsNullOrEmpty(headerBlock.ProjectTypeName)) return null;
	        return headerBlock;
        }
    }
}