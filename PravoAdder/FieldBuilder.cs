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

// ReSharper disable InconsistentlySynchronizedField

namespace PravoAdder
{
	public class FieldBuilder
	{
		private readonly HttpAuthenticator _httpAuthenticator;
		private readonly object _dictionaryContainsKeyLock = new object();

		private static Lazy<IList<Participant>> _participants;	
		private static IDictionary<string, ConcurrentBag<DictionaryItem>> _dictionaries;
		private static List<CalculationFormula> _formulas;

		public FieldBuilder(HttpAuthenticator httpAuthenticator)
		{
			_httpAuthenticator = httpAuthenticator;
			_participants = new Lazy<IList<Participant>>(() => ApiRouter.Participants.GetParticipants(_httpAuthenticator));
			_dictionaries = new ConcurrentDictionary<string, ConcurrentBag<DictionaryItem>>();
		}

		public object CreateFieldValueFromData(BlockFieldInfo fieldInfo, string fieldData, KeyValuePair<string, string> vad)
		{
			if (string.IsNullOrEmpty(fieldData)) return null;

			fieldData = fieldData.Replace("\"", "");
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
					if (fieldData == vad.Key) return GetParticipantFromData(fieldData, vad.Value);
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

		private Participant GetParticipantFromData(string fieldData, string vadId = null)
		{
			var correctFieldData = fieldData.Trim();

			if (_participants.Value.All(p => !p.Name.Equals(correctFieldData)))
			{
				var participant = ApiRouter.Participants.PutParticipant(_httpAuthenticator, fieldData, vadId);
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

			lock (_dictionaryContainsKeyLock)
			{
				if (!_dictionaries.ContainsKey(dictionaryName))
				{
					List<DictionaryItem> dictionaryItems;
					if (dictionaryName == "Currency")
					{
						dictionaryItems = ApiRouter.Currencies
							.GetCurrencies(_httpAuthenticator);
					}
					else
					{
						dictionaryItems = ApiRouter.Dictionary
							.GetDictionaryItems(_httpAuthenticator, fieldInfo.SpecialData)
							.Select(d => new DictionaryItem(FormatDictionaryItemName(d.Name), d.Id))
							.ToList();
					}
					
					_dictionaries.Add(dictionaryName, new ConcurrentBag<DictionaryItem>(dictionaryItems));
				}
			}

			if (dictionaryName == "Currency")
			{
				if (_dictionaries[dictionaryName].All(d => !d.LetterCode.Equals(correctName, StringComparison.InvariantCultureIgnoreCase))) return null;

				return _dictionaries[dictionaryName]
					.First(d => d.LetterCode == correctName);
			}

			if (_dictionaries[dictionaryName].All(d => !d.Name.Equals(correctName, StringComparison.InvariantCultureIgnoreCase)))
			{
				var dictionaryItem = ApiRouter.Dictionary
					.SaveDictionaryItem(_httpAuthenticator, dictionaryName, correctName);
				if (dictionaryItem == null) return null;

				_dictionaries[dictionaryName].Add(new DictionaryItem(correctName, dictionaryItem.Id));
			}

			return _dictionaries[dictionaryName]
				.First(d => d.Name == correctName);
		}
	}
}
