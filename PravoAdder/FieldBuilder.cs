using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
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

		public FieldBuilder(HttpAuthenticator httpAuthenticator)
		{
			_httpAuthenticator = httpAuthenticator;
			_participants = new Lazy<IList<Participant>>(() => ApiRouter.Participants.GetParticipants(_httpAuthenticator));
			_dictionaries = new ConcurrentDictionary<string, ConcurrentBag<DictionaryItem>>();
		}

		public object CreateFieldValueFromData(BlockFieldInfo fieldInfo, string fieldData)
		{
			if (string.IsNullOrEmpty(fieldData)) return null;

			fieldData = fieldData.Replace("\"", "");
			switch (fieldInfo.Type)
			{
				case "Value":
					return FormatFieldData(fieldData);
				case "Text":
					return fieldData;
				case "Formula":
					var calculationFormula = ApiRouter.CalculationFormulas
						.GetCalculationFormulas(_httpAuthenticator)
						.GetByName(fieldInfo.SpecialData);
					if (calculationFormula == null) return null;
					return new
					{
						Result = FormatFieldData(fieldData),
						CalculationFormulaId = calculationFormula.Id
					};
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
			string correctIntValue;
			if (!value.Contains(','))
			{
				correctIntValue = value;
			}
			else
			{
				var newValue = value.Replace(" ", "");
				var splitted = newValue.Split(',');
				correctIntValue = splitted[1].All(c => c == '0') ? splitted[0] : newValue;
			}

			if (TypeDescriptor.GetConverter(typeof(int)).IsValid(correctIntValue))
				return int.Parse(correctIntValue);

			if (TypeDescriptor.GetConverter(typeof(double)).IsValid(value.Replace(',', '.')))
				return double.Parse(value);
			return value;
		}

		private static string FormatDictionaryItemName(string item)
		{
			return $"{item.First().ToString().ToUpper()}{item.Substring(1)}";
		}

		private Participant GetParticipantFromData(string fieldData)
		{
			var participants = _participants.Value;
			var correctFieldData = fieldData.Trim();		
			Participant participant;

			if (participants.All(p => !p.Name.Equals(correctFieldData)))
			{
				participant = ApiRouter.Participants.PutParticipant(_httpAuthenticator, fieldData);
				if (participant == null) return null;

				participants = ApiRouter.Participants.GetParticipants(_httpAuthenticator);
			}

			participant = participants
				.First(p => p.Name == correctFieldData);

			return Participant.TryParse(participant);
		}

		private DictionaryItem GetDictionaryFromData(string fieldData, BlockFieldInfo fieldInfo)
		{
			var dictionaryName = fieldInfo.SpecialData;
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
				if (_dictionaries[dictionaryName].All(d => !d.LetterCode.Equals(correctName))) return null;

				return _dictionaries[dictionaryName]
					.First(d => d.LetterCode == correctName);
			}

			if (_dictionaries[dictionaryName].All(d => !d.Name.Equals(correctName)))
			{
				var dictionaryItem = ApiRouter.Dictionary
					.SaveDictionaryItem(_httpAuthenticator, fieldInfo.SpecialData, correctName);
				if (dictionaryItem == null) return null;

				_dictionaries[dictionaryName].Add(new DictionaryItem(correctName, dictionaryItem.Id));
			}

			return _dictionaries[dictionaryName]
				.First(d => d.Name == correctName);
		}
	}
}
