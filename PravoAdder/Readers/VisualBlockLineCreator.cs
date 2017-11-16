using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Repositories;
using PravoAdder.Domain;
using PravoAdder.Helpers;

namespace PravoAdder.Readers
{
	//
	// Need "Data Block", "Field Name", "Row", "Width", "Tag", "Required" in Excel table.
	//
	public class VisualBlockLineCreator : ICreator
	{
		public HttpAuthenticator HttpAuthenticator { get; }
		public VisualBlock VisualBlock { get; set; }
		public VisualBlockLine ConstructedLine { get; set; }

		public ICreatable Create(Row header, Row row, DatabaseEntityItem item = null)
		{
			var visualBlockName = Table.GetValue(header, row, "Data Block");

			string repeatingFieldValue;
			try
			{
				repeatingFieldValue = Table.GetValue(header, row, "Repeating Block");
			}
			catch (Exception)
			{
				repeatingFieldValue = null;
			}
			var isRepeatingBlock = bool.Parse(repeatingFieldValue ?? "false");

			var puttingVisualBlock = new VisualBlock
			{
				NameInConstructor = visualBlockName,
				IsRepeatable = isRepeatingBlock,
				Lines = new List<VisualBlockLine>()
			};
			VisualBlock = VisualBlockRepository.GetOrCreate<VisualBlockApi>(HttpAuthenticator, visualBlockName, puttingVisualBlock);

			var newField = GetVisualBlockField(HttpAuthenticator, header, row);
			if (newField == null) return null;

			VisualBlockLine line;
			if (ConstructedLine != null)
			{
				line = ConstructedLine.CloneJson();
				line.Fields.Add(newField);
			}
			else
			{
				var rowNamingRule = new Regex("[^a-яA-Яa-zA-Z ]");
				var rowType = rowNamingRule.Replace(Table.GetValue(header, row, "Row"), "").Trim();
				var lineType = _lineTypes.First(t => t.Name.Equals(rowType, StringComparison.InvariantCultureIgnoreCase));
				line = new VisualBlockLine
				{
					LineType = lineType,
					Fields = new List<VisualBlockField>(),
					Order = VisualBlock.Lines.Count, 
				};
				line.Fields.Add(newField);
			}

			return line;
		}

		private static List<ProjectFieldFormat> _formats;
		private static List<LineType> _lineTypes;

		public VisualBlockLineCreator(HttpAuthenticator httpAuthenticator)
		{
			HttpAuthenticator = httpAuthenticator;
			_formats = ApiRouter.Bootstrap.GetFieldTypes(httpAuthenticator);
			_lineTypes = ApiRouter.Bootstrap.GetLineTypes(HttpAuthenticator);
		}

		private static VisualBlockField GetVisualBlockField(HttpAuthenticator autenticator, Row header, Row row)
		{
			var fieldName = Table.GetValue(header, row, "Field Name")?.SliceSpaceIfMore(256);
			if (string.IsNullOrEmpty(fieldName)) return null;
			
			var newProjectField = new ProjectField
			{
				Name = fieldName,
				PlaceholderText = fieldName,
				ProjectFieldFormat = _formats.FirstOrDefault(f => f.Name.Equals("Text"))
			};
			var projectField = ProjectFieldRepository.GetOrCreate<ProjectFieldsApi>(autenticator, fieldName, newProjectField);
			
			return new VisualBlockField
			{
				IsRequired = bool.Parse(Table.GetValue(header, row, "Required")),
				Tag = Table.GetValue(header, row, "Tag").ToTag(),
				Width = int.Parse(Table.GetValue(header, row, "Width")),
				ProjectField = projectField
			};
		}
	}
}
