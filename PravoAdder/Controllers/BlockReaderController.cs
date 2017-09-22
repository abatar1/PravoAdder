﻿using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using PravoAdder.Domain;
using PravoAdder.Domain.Info;
using PravoAdder.Readers;
using PravoAdder.Api;
using PravoAdder.Readers.Color;
using PravoAdder.Readers.Simple;
using PravoAdder.Readers.Xml;

namespace PravoAdder.Controllers
{
    public class BlockReaderController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly BlockInfoReader _blockInfoReader;

        public BlockReaderController(Settings settings, HttpAuthenticator autentificator)
        {
            TableReader tableReader;
            switch (settings.BlockLoadingMode)
            {
                case "Color":
                    tableReader = new ColorExcelReader();
                    Table = tableReader.Read(settings);
                    _blockInfoReader = new ColorBlockInfoReader(Table, settings, autentificator);
                    break;
                case "Simple":
                    tableReader = new SimpleExcelReader();
                    Table = tableReader.Read(settings);
                    _blockInfoReader = new SimpleBlockInfoReader(Table, settings);
                    break;
				case "Xml":
					tableReader = new XmlWithMappingReader();
					Table = tableReader.Read(settings);
					_blockInfoReader = new ColorBlockInfoReader(Table, settings, autentificator);
					break;
                default:
                    var message = $"Типа блоков {settings.BlockLoadingMode} не существует.";
                    Logger.Error(message);
                    throw new ArgumentException(message);
            }
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