using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fclp.Internals.Extensions;
using OfficeOpenXml;
using PravoAdder.Domain;

namespace PravoAdder.Readers
{
	public class ExcelReferenceReader : TemplateTableReader
	{
		private class TableInfo : ExcelReferenceReader, IDisposable
		{
			public int TotalRows { get; }
			public int TotalColumns { get; }
			public Row Header { get; }
			public IDictionary<FieldAddress, int> HeaderRev { get; }
			public List<int> ColorHeader { get; }

			private ExcelWorksheet Worksheet { get; }
			private readonly ExcelPackage _excelPackage;

			public TableInfo(Settings settings, string filename)
			{
				var info = GetFileInfo(filename);
				_excelPackage = new ExcelPackage(info);

				Worksheet = _excelPackage.Workbook.Worksheets.First();

				TotalRows = Worksheet.Dimension.End.Row;
				TotalColumns = Worksheet.Dimension.End.Column;

				var infoRow = Worksheet
					.Cells[settings.InformationRowPosition, 1, settings.InformationRowPosition, TotalColumns];
				ColorHeader = infoRow
					.Where(c => settings.AllowedColor == c.Style.Fill.BackgroundColor.Rgb)
					.Select(c => c.Start.Column)
					.ToList();
				var headerContent = infoRow
					.Where(c => ColorHeader.Contains(c.Start.Column))
					.Select(c => FormatCell(c.Value) ?? string.Empty)
					.Zip(ColorHeader, (value, key) => new { Value = value, Key = key })
					.ToDictionary(key => key.Key, value => new FieldAddress(value.Value));
				Header = new Row(headerContent);
				HeaderRev = Header.ToDictionary(key => key.Value, value => value.Key);
			}

			protected sealed override FileInfo GetFileInfo(string name, params string[] stub)
			{
				return base.GetFileInfo(name, stub);
			}

			public void Dispose()
			{
				Worksheet.Dispose();
				_excelPackage.Dispose();
			}

			public string GetValue(int rowNum, int columnNum)
			{
				return Worksheet.Cells[rowNum, columnNum].Value?.ToString();
			}
		}

		protected override FileInfo GetFileInfo(string name, params string[] stub)
		{
			var extentions = new[] { ".xlsx", ".xlsb", ".xlsm" };
			return base.GetFileInfo(name, extentions);
		}				
	
		public override Table Read(Settings settings)
		{
			var mainTableInfo = new TableInfo(settings, settings.SourceName);
			var table = new List<Row>();
			var referenceTableInfos = new Dictionary<string, TableInfo>();

			for (var rowNum = settings.DataRowPosition + settings.RowNum - 1; rowNum <= mainTableInfo.TotalRows; rowNum++)
			{
				var row = new List<string>();
				var participants = new HashSet<string>();
				for (var columnNum = 1; columnNum <= mainTableInfo.TotalColumns; columnNum++)
				{				
					if (!mainTableInfo.Header.ContainsKey(columnNum)) continue;
					var fieldInfo = mainTableInfo.Header[columnNum];

					var cellValue = mainTableInfo.GetValue(rowNum, columnNum);
					if (fieldInfo.IsReference)					
					{
						if (cellValue == null)
						{
							row.Add(null);
							continue;
						}

						var cellParts = cellValue.Split('\n');
						if (cellParts.Any(c => c.Contains('(')))
						{
							var newParticipants = new HashSet<string>();
							newParticipants.UnionWith(cellParts
								.Where(c => c.Contains("Client"))
								.Skip(1));
							newParticipants.UnionWith(cellParts
								.Where(c => !c.Contains("Client")));
							participants.UnionWith(newParticipants
								.Select(c => c.Remove(c.IndexOf("(", StringComparison.Ordinal))));
						}						

						if (cellParts.Any(c => c.Contains("Client")))
						{
							cellValue = cellParts
								.First(c => c.Contains("Client"))
								.Replace(" (Client)", "");						
						}
						else
						{
							cellValue = "Noelle M. Melanson";
						}

						var reference = fieldInfo.Reference;
						if (!referenceTableInfos.ContainsKey(reference))
						{
							referenceTableInfos.Add(fieldInfo.Reference, new TableInfo(settings, fieldInfo.Reference));
						}
						var reversedHeader = referenceTableInfos[reference].HeaderRev;
						if (!reversedHeader.ContainsKey(fieldInfo))
						{
							row.Add(null);
							continue;
						}

						var keyPos = reversedHeader[reversedHeader.Keys.First(x => x.IsKey)];
						var fieldPos = reversedHeader[fieldInfo];

						for (var rowNum1 = 2; rowNum1 <= referenceTableInfos[reference].TotalRows; rowNum1++)
						{
							var cellValues = referenceTableInfos[reference].GetValue(rowNum1, keyPos);
							if (cellValues == null) continue;
							if (!cellValues.Split('\n').Contains(cellValue)) continue;

							cellValue = referenceTableInfos[reference].GetValue(rowNum1, fieldPos);
							break;
						}
					}
					
					row.Add(FormatCell(cellValue));
				}
				var coloredRow = row
					.Zip(Enumerable.Range(1, mainTableInfo.TotalColumns), (value, index) => new { value, index })
					.Where(z => mainTableInfo.ColorHeader.Contains(z.index))
					.ToDictionary(key => key.index, value => new FieldAddress(value.value));
				table.Add(new Row(coloredRow));
			}

			referenceTableInfos.ForEach(info => info.Value.Dispose());
			mainTableInfo.Dispose();
			return new Table(table, mainTableInfo.Header);
		}		
	}
}
