using System;
using System.Collections.Generic;
using System.Linq;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Repositories;
using PravoAdder.Domain;
using PravoAdder.Helpers;

namespace PravoAdder.Readers
{
	public class ProjectFieldCreator : Creator
	{
		private static List<ProjectFieldFormat> _formats;

		private static ProjectFieldFormat GetSimpleFormat(string name)
		{
			return _formats.FirstOrDefault(f => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
		}

		private static ProjectFieldFormat GetComplexFormat(string parentName, string name)
		{
			return _formats
				.First(f => f.Name.Equals(parentName)).Childrens
				.First(f => f.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
		}

		public override ICreatable Create(Row header, Row row, DatabaseEntityItem item = null)
		{
			var projectField = new ProjectField
			{
				Name = Table.GetValue(header, row, "Field Name").SliceSpaceIfMore(256),
				PlaceholderText = Table.GetValue(header, row, "Placeholder Text").SliceSpaceIfMore(256),
				Tag = Table.GetValue(header, row, "Tag").ToTag()
			};

			var rawFormat = Table.GetValue(header, row, "Data Format");
			if (string.IsNullOrEmpty(rawFormat) || string.IsNullOrEmpty(projectField.Name) || string.IsNullOrEmpty(projectField.PlaceholderText))
			{
				return null;
			}
			var format = rawFormat.Split(',').Select(c => c.Trim()).ToArray();
			var fieldFormat = format[0];

			if (_formats == null) _formats = ApiRouter.Bootstrap.GetFieldTypes(HttpAuthenticator);

			ProjectFieldFormat projectFieldFormat;
			switch (fieldFormat)
			{
				case "Dictionary":
					var dictionary = DictionaryRepository.GetOrCreate<DictionaryApi>(HttpAuthenticator, format[1], new DictionaryInfo { Name = format[1] });
					projectFieldFormat = _formats.First(f => f.Name.Equals(fieldFormat));
					projectFieldFormat.Dictionary = dictionary;
					break;
				case "Participant":
					projectFieldFormat = GetComplexFormat("Object", fieldFormat);
					break;
				case "Object":
					projectFieldFormat = GetComplexFormat(fieldFormat, format[1]);
					break;
				case "Text":
				case "Date":				
				case "Value":
				case "Number":
				case "Text Area":
					projectFieldFormat = GetSimpleFormat(fieldFormat);
					break;
				case "TDate":
					projectFieldFormat = GetSimpleFormat("Date");
					break;
				case "Numbers":
					projectFieldFormat = GetSimpleFormat("Number");
					break;
				default:
					throw new ArgumentException($"Тип формата {fieldFormat} не поддерживается.");
			}
			projectField.ProjectFieldFormat = projectFieldFormat;

			return projectField;
		}

		public ProjectFieldCreator(HttpAuthenticator httpAuthenticator, ApplicationArguments applicationArguments) : base(httpAuthenticator, applicationArguments)
		{
		}
	}
}
