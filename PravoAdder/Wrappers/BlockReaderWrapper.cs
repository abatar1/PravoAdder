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

        public BlockReaderWrapper(ApplicationArguments args, Settings settings, HttpAuthenticator autentificator)
        {
            TemplateTableReader tableReader;
            switch (args.ReaderMode)
            {
                case ReaderMode.Excel:
                    tableReader = new ExcelReader();
                    Table = tableReader.Read(args, settings);
                    break;
				case ReaderMode.XmlMap:
					tableReader = new XmlMappingReader();
					Table = tableReader.Read(args, settings);
					break;
				case ReaderMode.ExcelRule:
					tableReader = new ExcelRuleReader();
					Table = tableReader.Read(args, settings);
					break;
	            case ReaderMode.ExcelReference:
		            tableReader = new ExcelReferenceReader();
		            Table = tableReader.Read(args, settings);
		            break;
				default:
                    var message = $"Типа блоков {args.ReaderMode} не существует.";
                    Logger.Error(message);
                    throw new ArgumentException(message);
            }
			_blockInfoReader = new BlocksConstructor(Table, settings, autentificator);
		}

        public Table Table { get; }

        public IList<CaseInfo> ReadBlockInfo()
        {
            return _blockInfoReader.CreateCaseInfo().ToList();
        }

        public HeaderBlockInfo ReadHeader(Row tableRow)
        {
            var headerBlock = _blockInfoReader.ReadHeaderBlock(tableRow);
	        if (string.IsNullOrEmpty(headerBlock.ProjectName) || string.IsNullOrEmpty(headerBlock.ProjectTypeName)) return null;
	        return headerBlock;
        }
    }
}