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

namespace PravoAdder
{
	public class FieldBuilder
	{
		private readonly HttpAuthenticator _httpAuthenticator;
		private static Lazy<IList<Participant>> _participants;	
		private static ConcurrentDictionary<string, ConcurrentBag<DictionaryItem>> _dictionaries;
		private static List<CalculationFormula> _formulas;

		public FieldBuilder(HttpAuthenticator httpAuthenticator)
		{
			_httpAuthenticator = httpAuthenticator;
			_participants = new Lazy<IList<Participant>>(() => ApiRouter.Participants.GetParticipants(_httpAuthenticator));
			_dictionaries = new ConcurrentDictionary<string, ConcurrentBag<DictionaryItem>>();
		}

		public object CreateFieldValueFromData(BlockFieldInfo fieldInfo, string fieldData)
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
					return GetCalculationFormulaValueFromData(fieldData, fieldInfo.SpecialData);
				case "Dictionary":
					return GetDictionaryFromData(fieldData, fieldInfo);
				case "Participant":
					return GetParticipantFromData(fieldData);
				default:
					throw new ArgumentException("Unknown type of value.");
			}
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

		private CalculationFormulaValue GetCalculationFormulaValueFromData(string data, string specialData)
		{
			if (_formulas == null)
			{
				_formulas = ApiRouter.CalculationFormulas
					.GetCalculationFormulas(_httpAuthenticator)
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

		private Participant GetParticipantFromData(string fieldData)
		{
			var correctFieldData = fieldData.Trim();

			if (_participants.Value.All(p => !p.Name.Equals(correctFieldData)))
			{
				var participant = ApiRouter.Participants.PutParticipant(_httpAuthenticator, fieldData);
				if (participant == null) return null;
				_participants.Value.Add(participant);
			}
			return _participants.Value
				.First(p => p.Name == correctFieldData);
		}

		private DictionaryItem GetDictionaryFromData(string fieldData, BlockFieldInfo fieldInfo)
		{
			var dictionaryName = fieldInfo.SpecialData.Trim();
			var correctName = FormatDictionaryItemName(fieldData);

			List<DictionaryItem> dictionaryItems;
			if (dictionaryName == "Currency")
			{
				dictionaryItems = ApiRouter.Currencies.GetCurrencies(_httpAuthenticator);
			}
			else
			{
				dictionaryItems = ApiRouter.Dictionary.GetDictionaryItems(_httpAuthenticator, fieldInfo.SpecialData)
					.Select(d => new DictionaryItem(FormatDictionaryItemName(d.Name), d.Id))
					.ToList();
			}
			_dictionaries.AddOrUpdate(dictionaryName, new ConcurrentBag<DictionaryItem>(dictionaryItems),
				(s, bag) => new ConcurrentBag<DictionaryItem>(bag.Concat(dictionaryItems)));

			if (!_dictionaries.TryGetValue(dictionaryName, out var itemsBag)) return null;

			bool InvEqual(string s1, string s2) => s1.Equals(s2, StringComparison.InvariantCultureIgnoreCase);
			if (dictionaryName == "Currency")
			{
				if (itemsBag.All(d => !InvEqual(d.LetterCode, correctName))) return null;
				return itemsBag.First(d => d.LetterCode == correctName);
			}

			if (itemsBag.All(d => !InvEqual(d.Name, correctName)))
			{
				var dictionaryItem = ApiRouter.Dictionary.SaveDictionaryItem(_httpAuthenticator, dictionaryName, correctName);
				if (dictionaryItem == null) return null;

				_dictionaries[dictionaryName].Add(new DictionaryItem(correctName, dictionaryItem.Id));
			}

			return itemsBag
				.First(d => d.Name == correctName);
		}
	}
}
