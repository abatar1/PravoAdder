using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;
using PravoAdder.Domain;

namespace PravoAdder.Helpers
{
	public class FieldBuilder
	{
		private static Lazy<IList<Participant>> _participants;	
		private static readonly ConcurrentDictionary<string, ConcurrentBag<DictionaryItem>> Dictionaries;
		private static List<CalculationFormula> _formulas;

		static FieldBuilder()
		{
			Dictionaries = new ConcurrentDictionary<string, ConcurrentBag<DictionaryItem>>();
		}

		public static object CreateFieldValueFromData(HttpAuthenticator httpAuthenticator, BlockFieldInfo fieldInfo, string fieldData)
		{
			if (string.IsNullOrEmpty(fieldData)) return null;
			switch (fieldInfo.Type)
			{
				case "Value":
					return FormatFieldData(fieldData);
				case "Text":
					if (fieldData == "True") return "Да";
					if (fieldData == "False") return "Нет";
					return fieldData;
				case "CalculationFormula":
					return GetCalculationFormulaValueFromData(httpAuthenticator, fieldData, fieldInfo.SpecialData);
				case "Dictionary":
					return GetDictionaryFromData(httpAuthenticator, fieldData, fieldInfo.SpecialData);
				case "Participant":
					return GetParticipantFromData(httpAuthenticator, fieldData);
				default:
					throw new ArgumentException("Unknown type of value.");
			}
		}

		public static object CreateFieldValueFromData(HttpAuthenticator httpAuthenticator, VisualBlockField visualField, string fieldData)
		{
			return CreateFieldValueFromData(httpAuthenticator, BlockFieldInfo.Create(visualField, 0), fieldData);
		}

		private static object FormatFieldData(string value)
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
			if (_formulas == null)
			{
				_formulas = ApiRouter.CalculationFormulas
					.GetCalculationFormulas(httpAuthenticator)
					.ToList();
			}
			var calculationFormula = _formulas.GetByName(specialData);
			if (calculationFormula == null) return null;
			return new CalculationFormulaValue
			{
				Result = FormatFieldData(data),
				CalculationFormulaId = calculationFormula.Id
			};
		}

		private static Participant GetParticipantFromData(HttpAuthenticator httpAuthenticator, string fieldData)
		{
			if (_participants == null)
			{
				_participants = new Lazy<IList<Participant>>(() => ApiRouter.Participants.GetParticipants(httpAuthenticator));
			}
			var correctFieldData = fieldData.Trim();

			if (_participants.Value.All(p => !p.Name.Equals(correctFieldData)))
			{
				var participant = ApiRouter.Participants.PutParticipant(httpAuthenticator, fieldData);
				if (participant == null) return null;
				_participants.Value.Add(participant);
			}
			return _participants.Value
				.First(p => p.Name == correctFieldData);
		}

		private static DictionaryItem GetDictionaryFromData(HttpAuthenticator httpAuthenticator, string fieldData, string dictionaryName)
		{
			dictionaryName = dictionaryName.Trim();
			var correctName = FormatDictionaryItemName(fieldData);

			if (!Dictionaries.ContainsKey(dictionaryName))
			{
				List<DictionaryItem> dictionaryItems;

				if (dictionaryName == "Currency")
				{
					dictionaryItems = ApiRouter.Currencies.GetCurrencies(httpAuthenticator);
				}
				else
				{
					dictionaryItems = ApiRouter.Dictionary.GetDictionaryItems(httpAuthenticator, dictionaryName)
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
				if (itemsBag.All(d => !InvEqual(d.LetterCode, correctName))) return null;
				return itemsBag.First(d => d.LetterCode == correctName);
			}

			if (itemsBag.All(d => !InvEqual(d.Name, correctName)))
			{
				var dictionaryItem = ApiRouter.Dictionary.SaveDictionaryItem(httpAuthenticator, dictionaryName, correctName);
				if (dictionaryItem == null) return null;

				Dictionaries[dictionaryName].Add(new DictionaryItem(correctName, dictionaryItem.Id));
			}

			return itemsBag
				.First(d => d.Name == correctName);
		}
	}
}
