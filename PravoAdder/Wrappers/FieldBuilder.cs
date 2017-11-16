using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Repositories;

namespace PravoAdder.Wrappers
{
	public class FieldBuilder
	{
		private static readonly ConcurrentDictionary<string, ConcurrentBag<DictionaryItem>> Dictionaries;

		static FieldBuilder()
		{
			Dictionaries = new ConcurrentDictionary<string, ConcurrentBag<DictionaryItem>>();
		}

		public static object CreateFieldValueFromData(HttpAuthenticator httpAuthenticator, VisualBlockField fieldInfo, string fieldData)
		{
			if (string.IsNullOrEmpty(fieldData)) return null;
			switch (fieldInfo.ProjectField.ProjectFieldFormat.SysName)
			{
				case "Number":
					return GetNumberFromData(fieldData);
				case "Text":
				case "Date":
				case "TextArea":
					if (fieldData == "True") return "Да";
					if (fieldData == "False") return "Нет";
					return fieldData;
				case "CalculationFormula":
					return GetCalculationFormulaValueFromData(httpAuthenticator, fieldData, fieldInfo.ProjectField.ProjectFieldFormat.Dictionary.SystemName);
				case "Dictionary":
					return GetDictionaryFromData(httpAuthenticator, fieldData, fieldInfo.ProjectField.CalculationFormulas.First().Name);
				case "Participant":
					return GetParticipantFromData(httpAuthenticator, fieldData);
				default:
					throw new ArgumentException("Unknown type of value.");
			}
		}

		private static object GetNumberFromData(string value)
		{
			string correctNumberValue;
			if (!value.Contains(',') && !value.Contains('.'))
			{
				correctNumberValue = value.Trim();
			}
			else
			{
				var trimmed = string.Join("", value.Where(c => !char.IsWhiteSpace(c)));
				var newValue = trimmed.Replace(",", ".");
				var splitted = newValue.Split('.');
				correctNumberValue = splitted[1].All(c => c == '0') ? splitted[0] : newValue;
			}			

			if (TypeDescriptor.GetConverter(typeof(int)).IsValid(correctNumberValue))
			{
				return int.Parse(correctNumberValue, CultureInfo.InvariantCulture);
			}

			if (TypeDescriptor.GetConverter(typeof(double)).IsValid(correctNumberValue))
			{
				return double.Parse(correctNumberValue, CultureInfo.InvariantCulture);
			}
				
			return value;
		}

		private static string FormatDictionaryItemName(string item)
		{
			return $"{item.First().ToString().ToUpper()}{item.Substring(1)}".Trim();
		}

		private static CalculationFormulaValue GetCalculationFormulaValueFromData(HttpAuthenticator httpAuthenticator, string data, string specialData)
		{
			var calculationFormula = CalculationRepository.Get<CalculationFormulasApi>(httpAuthenticator, specialData);
			if (calculationFormula == null) return null;
			return new CalculationFormulaValue
			{
				Result = GetNumberFromData(data),
				CalculationFormulaId = calculationFormula.Id
			};
		}

		private static Participant GetParticipantFromData(HttpAuthenticator httpAuthenticator, string participantName)
		{
			// TODO доделать создание
			return ParticipantsRepository.GetOrCreate<ParticipantsApi>(httpAuthenticator, participantName,
				new Participant());
		}

		private static DictionaryItem GetDictionaryFromData(HttpAuthenticator httpAuthenticator, string fieldData, string dictionaryName)
		{
			dictionaryName = dictionaryName.Trim();
			var correctItemName = FormatDictionaryItemName(fieldData);

			if (!Dictionaries.ContainsKey(dictionaryName))
			{
				List<DictionaryItem> dictionaryItems;

				if (dictionaryName == "Currency")
				{
					dictionaryItems = ApiRouter.Currencies.GetMany(httpAuthenticator);
				}
				else
				{
					dictionaryItems = ApiRouter.DictionaryItems.GetMany(httpAuthenticator, dictionaryName)
						.Select(d => new DictionaryItem(FormatDictionaryItemName(d.Name), d.Id))
						.ToList();
				}
				Dictionaries.AddOrUpdate(dictionaryName, new ConcurrentBag<DictionaryItem>(dictionaryItems),
					(s, bag) => new ConcurrentBag<DictionaryItem>(bag.Concat(dictionaryItems)));
			}		

			if (!Dictionaries.TryGetValue(dictionaryName, out var itemsBag)) return null;

			bool InvEqual(string s1, string s2) => s1.Equals(s2, StringComparison.InvariantCultureIgnoreCase);
			if (dictionaryName == "Currency")
			{
				if (itemsBag.All(d => !InvEqual(d.LetterCode, correctItemName))) return null;
				return itemsBag.First(d => d.LetterCode == correctItemName);
			}

			if (itemsBag.All(d => !InvEqual(d.Name, correctItemName)))
			{
				var dictionaryItem = ApiRouter.DictionaryItems.Create(httpAuthenticator,
					new DictionaryItem {SystemName = dictionaryName, Name = correctItemName});
				if (dictionaryItem == null) return null;

				Dictionaries[dictionaryName].Add(new DictionaryItem(correctItemName, dictionaryItem.Id));
			}

			return itemsBag
				.First(d => d.Name == correctItemName);
		}
	}
}
