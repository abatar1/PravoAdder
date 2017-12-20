using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PravoAdder.Domain;

namespace PravoAdder.Readers
{
	public class ExcelRuleReader : TemplateTableReader
	{
		protected override FileInfo GetFileInfo(string name, params string[] stub)
		{
			var extentions = new[] { ".xlsx", ".xlsb", ".xlsm" };
			return base.GetFileInfo(name, extentions);
		}

		public override Table Read(Settings settings)
		{
			var table = new ExcelReader(settings.SourceName).Read(settings);

			var keyIndex = GetIndexByName(table.Header.Content, "Номер дела");
			var responseIndex = GetIndexByName(table.Header.Content, "Ответственный");

			var rgx = new Regex(@"^А\d{2}-[0-9]+/20\d{2}");
			var groupedTable = table.TableContent
				.GroupBy(row => row.First(cell => cell.Key == keyIndex).Value)
				.Where(row => row.Key.Value != null && rgx.IsMatch(row.Key.Value))
				.Select(row => row.Last()
					.Select(cell => 
					{
						if (cell.Key == responseIndex)
						{
							if (cell.Value.Value == null || cell.Value.Value == "Общее")
							{
								return new KeyValuePair<int, FieldAddress>(cell.Key, new FieldAddress("Неназначенный Ответственный", settings.FieldReadingMode));
							}
						}
						return cell;
					})
					.ToDictionary(key => key.Key, value => 
					{
						var isDate = DateTime.TryParseExact(value.Value.Value, "dd.MM.yyyy h:mm:ss", System.Globalization.CultureInfo.InvariantCulture,
							System.Globalization.DateTimeStyles.None, out var dateTime);
						return new FieldAddress(FormatCell(isDate ? dateTime : (object) value.Value), settings.FieldReadingMode);
					}))
				.Select(row => new Row(row))
				.ToList();
			return new Table(groupedTable, table.Header);		
		}

		private static int GetIndexByName(Dictionary<int, FieldAddress> row, string name)
		{
			return row.First(x => x.Value.FieldName == name).Key;
		}
	}
}
