using System;
using System.Collections.Generic;
using System.Linq;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Domain;
using PravoAdder.Helpers;

namespace PravoAdder.Readers
{
	public class ProjectFieldCreator : ICreator
	{
		public ProjectFieldCreator(HttpAuthenticator httpAuthenticator)
		{
			HttpAuthenticator = httpAuthenticator;
		}

		public HttpAuthenticator HttpAuthenticator { get; }
		private static List<DictionaryInfo> _dictionaries;
		private static List<ProjectFieldFormat> _formats;

		public ICreatable Create(Row header, Row row)
		{
			var projectField = new ProjectField
			{
				Name = Table.GetValue(header, row, "Field Name").SliceSpaceIfMore(256),
				PlaceholderText = Table.GetValue(header, row, "Placeholder Text").SliceSpaceIfMore(256)
			};

			var rawFormat = Table.GetValue(header, row, "Data Format");
			if (string.IsNullOrEmpty(rawFormat) || string.IsNullOrEmpty(projectField.Name) || string.IsNullOrEmpty(projectField.PlaceholderText))
			{
				return null;
			}
			var format = rawFormat.Split(',').Select(c => c.Trim()).ToArray();
			var fieldFormat = format[0];

			if (_formats == null) _formats = ApiRouter.VisualBlock.GetFieldTypes(HttpAuthenticator);

			switch (fieldFormat)
			{
				case "Dictionary":
					if (_dictionaries == null) _dictionaries = ApiRouter.Dictionary.GetDictionaryList(HttpAuthenticator);
					var dictionary =
						_dictionaries.FirstOrDefault(d => d.DisplayName.Equals(format[1], StringComparison.InvariantCultureIgnoreCase));
					if (dictionary == null)
					{
						dictionary = ApiRouter.Dictionary.Create(HttpAuthenticator, new DictionaryInfo {Name = format[1]});
						_dictionaries.Add(dictionary);
					}

					projectField.ProjectFieldFormat = _formats.First(f => f.Name.Equals(fieldFormat));
					projectField.ProjectFieldFormat.Dictionary = _dictionaries
							.FirstOrDefault(d => d.DisplayName.Equals(format[1], StringComparison.InvariantCultureIgnoreCase));
					break;
				case "Object":
					projectField.ProjectFieldFormat = _formats
						.First(f => f.Name.Equals(fieldFormat)).Childrens
						.First(f => f.Name.Equals(format[1], StringComparison.CurrentCultureIgnoreCase));
					break;
				case "Text":
				case "Date":
				case "Value":
				case "Number":
				case "Text Area":
					var currentFormat =
						_formats.FirstOrDefault(f => f.Name.Equals(fieldFormat, StringComparison.InvariantCultureIgnoreCase));
					projectField.ProjectFieldFormat = currentFormat;
					break;
				default:
					throw new ArgumentException("Тип формата не поддерживается.");
			}			
			return projectField;
		}
	}
}
