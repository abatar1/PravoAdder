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

		public ICreatable Create(Row header, Row row)
		{
			var projectField = new ProjectField
			{
				Name = Table.GetValue(header, row, "Field name").SliceSpaceIfMore(256),
				PlaceholderText = Table.GetValue(header, row, "Placeholder text").SliceSpaceIfMore(256)
			};

			var format = Table.GetValue(header, row, "Data format").Split(',').Select(c => c.Trim()).ToArray();
			var fieldFormat = format[0];
			switch (fieldFormat)
			{
				case "Dictionary":
					if (_dictionaries == null) _dictionaries = ApiRouter.Dictionary.GetDictionaryList(HttpAuthenticator);
					projectField.ProjectFieldFormat = new ProjectFieldFormat
					{
						SysName = fieldFormat,
						Dictionary = _dictionaries
							.FirstOrDefault(d => d.DisplayName.Equals(format[1], StringComparison.InvariantCultureIgnoreCase))
					};
					break;
				default:
					throw new ArgumentException("Тип формата не поддерживается.");
			}			
			return projectField;
		}
	}
}
