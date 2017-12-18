using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Fclp.Internals.Extensions;
using PravoAdder.Domain;
using PravoAdder.Helpers;

namespace PravoAdder.Readers
{
	public class ExcelSplitTables : TemplateTableReader
	{
		private static void AddSimpleFieldToRow(Table table, int i, IDictionary<int, FieldAddress> row, string fieldName, string blockName, Row header)
		{
			if (i >= table.Size) return;

			var address = new FieldAddress(blockName, fieldName);
			address.Value = Table.GetValue(table.Header, table.TableContent[i], address);
			row.Add(GetIndex(header, address), new FieldAddress {Value = Table.GetValue(table.Header, table.TableContent[i], address)});
		}

		private static string TryAddSimpleFieldToRow(Table table, int i, IDictionary<int, FieldAddress> row, FieldAddress address, Row header)
		{
			if (i >= table.Size) return string.Empty;

			var value = Table.GetValue(table.Header, table.TableContent[i], address);
			if (string.IsNullOrEmpty(value)) return string.Empty;

			address.Value = value;			

			var index = GetIndex(header, address);
			row.Add(index, new FieldAddress { Value = Table.GetValue(table.Header, table.TableContent[i], address) });
			return value;
		}

		private static int GetIndex(Row header, FieldAddress address)
		{
			return header.First(f => f.Value.Equals(address)).Key;
		}

		private static IEnumerable<string> GetMatches(Table table, int index, FieldAddress address)
		{
			if (index >= table.Size) yield break;

			var re = new Regex(@"^[ФС №|АС №|Серия АС №]*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
			var value = Table.GetValue(table.Header, table.TableContent[index], address);

			foreach (var line in value.Split('\n'))
			{
				if (re.IsMatch(line)) yield return line;
			}			
		}

		private static FieldAddress GetContactField(Table table, FieldAddress fieldAddress, int i)
		{
			if (i >= table.Size) return new FieldAddress();

			var value = Table.GetValue(table.Header, table.TableContent[i], fieldAddress);

			var telephones = new StringBuilder();
			var name = string.Empty;
			foreach (var line in value.Split('\n'))
			{
				// Check if match telephone number rule
				var potentialNumber = new string(line.Where(char.IsDigit).ToArray());
				if (potentialNumber.Length == 11)
				{
					telephones.Append(line.Trim()).Append("; ");
					continue;
				}

				// Check if match name rule
				if (line.Contains("Судебный пристав-исполнитель"))
				{
					name = line.Trim();
					continue;
				}
				var potentialName = line.Split(' ');
				if (potentialName.Length == 3)
				{
					if (potentialName.Skip(1).All(w => w.EndsWith(".")) ||  potentialName.All(c => char.IsUpper(c.First())))
					{
						name = line.Trim();
					}
				}
			}

			return new FieldAddress {Value = $"{name}; {telephones}"};
		}

		private static FieldAddress GetIlField(Table firstTable, Table secondTable, FieldAddress address, int i, int j)
		{
			var value = string.Empty;

			List<string> matches;
			if (j != firstTable.Size)
			{
				matches = GetMatches(firstTable, j, address).ToList();
				if (matches.Count > 0)
				{
					value = matches[matches.Count - 1];
				}			
			}			

			matches = GetMatches(secondTable, i, address).ToList();
			if (matches.Count > 0)
			{
				value = !string.IsNullOrEmpty(value) ? string.Join("\n", matches) : matches[matches.Count - 1];
			}				

			return new FieldAddress {Value = value };
		}

		private static IEnumerable<KeyValuePair<int, FieldAddress>> GetRepeatingRows(Table table, FieldAddress address, int i, Row header)
		{			
			var value = Table.GetValue(table.Header, table.TableContent[i], address);
			var newFields = value.Split('\n')
				.Where(c => !string.IsNullOrEmpty(c))
				.ToList();

			var fieldCount = header.Values.Count(x => x.Equals(address));
			if (fieldCount == 1)
			{
				header[table.TryGetIndex(address)].RepeatFieldNumber = 1;
			}

			if (newFields.Count > fieldCount)
			{
				var currentRepeatNumber = header
					.Where(x => x.Value.Equals(address))
					.Max(x => x.Value.RepeatFieldNumber);

				var numbersToNewFields = newFields.Count - fieldCount;
				Enumerable.Range(currentRepeatNumber + 1, numbersToNewFields)
					.Select(x => new FieldAddress(address.BlockName, address.FieldName) { RepeatFieldNumber = x })
					.ForEach(header.Add);
			}

			foreach (var repeatNumber in Enumerable.Range(1, newFields.Count))
			{
				var index = header
					.Where(x => x.Value.Equals(address))
					.First(x => x.Value.RepeatFieldNumber.Equals(repeatNumber)).Key;
				yield return new KeyValuePair<int, FieldAddress>(index, new FieldAddress {Value = newFields[repeatNumber - 1]});
			}
		}

		public override Table Read(Settings settings)
		{
			if (string.IsNullOrEmpty(settings.SecondSourceName) || string.IsNullOrEmpty(settings.SourceName)) return null;
			
			var firstTable = new ExcelReader(settings.SourceName).Read(settings);
			var secondTable = new ExcelReader(settings.SecondSourceName).Read(settings);

			var firstContent = firstTable.TableContent;
			var secondContent = secondTable.TableContent;

			const string blockName = "Исполнительное производство";
			var projectAddress = new FieldAddress("Системный", "Название дела");

			var fHeaderExceptProjectName = firstTable.Header.Where(x => !x.Value.Equals(projectAddress));	
			var newHeader = secondTable.Header.Concat(new Row(fHeaderExceptProjectName));								

			var newContent = new List<Row>();

			for (var i = 0; i < Math.Max(firstContent.Count, secondContent.Count); i++)
			{
				var newRow = new Dictionary<int, FieldAddress>();
				
				var projectName = TryAddSimpleFieldToRow(secondTable, i, newRow, projectAddress, newHeader);

				if (string.IsNullOrEmpty(projectName)) continue;

				var projectFtIndex = GetIndex(firstTable.Header, projectAddress);

				var j = firstContent
					.TakeWhile(x =>
					{
						if (x.ContainsKey(projectFtIndex))
						{
							return !x[projectFtIndex].Value.Contains(projectName);
						}
						return true;
					})
					.Count();
				if (j != firstContent.Count)
				{
					AddSimpleFieldToRow(firstTable, j, newRow, "Сумма по ИЛ", blockName, newHeader);
					AddSimpleFieldToRow(firstTable, j, newRow, "Удовлетворено", blockName, newHeader);
					AddSimpleFieldToRow(firstTable, j, newRow, "Остаток задолженности", blockName, newHeader);
					AddSimpleFieldToRow(firstTable, j, newRow, "Взыскано", blockName, newHeader);					

					var addressSpi = new FieldAddress(blockName, "СПИ");
					if (i < firstTable.Size)
					{
						newRow.Add(GetIndex(newHeader, addressSpi), new FieldAddress
						{
							Value = Table.GetValue(firstTable.Header, firstTable.TableContent[i], addressSpi)
								.After("Подразделение ССП:")
						});
					}

					var addressC = new FieldAddress(blockName, "Контакты СПИ");
					newRow.Add(GetIndex(newHeader, addressC), GetContactField(firstTable, addressC, j));

					// Adding repeating rows
					var repeat1 = GetRepeatingRows(firstTable, new FieldAddress(blockName, "Деятельность в рамках ИП"), j, newHeader)
						.ToDictionary(x => x.Key, x => x.Value);

					var repeat2 = GetRepeatingRows(firstTable, new FieldAddress(blockName, "Дата действия"), j, newHeader)
						.ToDictionary(x => x.Key, x => x.Value);

					newRow = newRow.ConcatFromDictionary(repeat1);
					newRow = newRow.ConcatFromDictionary(repeat2);
				}

				var addressIl = new FieldAddress(blockName, "Номер ИЛ");
				newRow.Add(GetIndex(newHeader, addressIl), GetIlField(firstTable, secondTable, addressIl, i, j));

				AddSimpleFieldToRow(secondTable, i, newRow, "Информация об исполнительном листе", blockName, newHeader);							
										
				newContent.Add(new Row(newRow));				
			}

			return new Table(newContent, newHeader);
		}
	}
}
