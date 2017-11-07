﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Domain;
using PravoAdder.Helpers;

namespace PravoAdder.Readers
{
	public class VisualBlockLineCreator : ICreator
	{
		public HttpAuthenticator HttpAuthenticator { get; }
		public VisualBlock VisualBlock { get; set; }
		public VisualBlockLine ConstructedLine { get; set; }

		public ICreatable Create(Row header, Row row)
		{
			VisualBlockLine line;

			if (_visualBlocks == null) _visualBlocks = ApiRouter.VisualBlock.GetMany(HttpAuthenticator);
			var visualBlockName = Table.GetValue(header, row, "Data Block");
		
			if (string.IsNullOrEmpty(visualBlockName)) return null;

			VisualBlock = _visualBlocks.FirstOrDefault(vb => vb.Name.Equals(visualBlockName));
			if (VisualBlock == null)
			{
				VisualBlock = ApiRouter.VisualBlock.Create(HttpAuthenticator,
					new VisualBlock
					{
						NameInConstructor = visualBlockName,
						IsRepeatable = false,
						Lines = new List<VisualBlockLine>()
					});
				_visualBlocks.Add(VisualBlock);
			}

			var newField = GetVisualBlockField(HttpAuthenticator, header, row);
			if (newField == null) return null;

			if (ConstructedLine != null)
			{
				line = ConstructedLine.CloneJson();
				line.Fields.Add(newField);
			}
			else
			{
				if (_lineTypes == null)
					_lineTypes = ApiRouter.VisualBlock.GetLineTypes(HttpAuthenticator);
				var lineType = _lineTypes.First(t => t.Name.Equals(Table.GetValue(header, row, "Row"),
					StringComparison.InvariantCultureIgnoreCase));
				line = new VisualBlockLine
				{
					LineType = lineType,
					Fields = new List<VisualBlockField>(),
					Order = VisualBlock.Lines.Count
				};
				line.Fields.Add(newField);
			}

			return line;
		}

		private static Random _random;
		private static List<ProjectFieldFormat> _formats;
		private static List<VisualBlock> _visualBlocks;
		private static List<LineType> _lineTypes;

		public VisualBlockLineCreator(HttpAuthenticator httpAuthenticator)
		{
			HttpAuthenticator = httpAuthenticator;
		}

		private static VisualBlockField GetVisualBlockField(HttpAuthenticator autentificator, Row header, Row row)
		{
			var name = Table.GetValue(header, row, "Field Name")?.SliceSpaceIfMore(256);
			if (string.IsNullOrEmpty(name)) return null;

			var projectField = ApiRouter.ProjectFields.GetMany(autentificator, name).FirstOrDefault(f => f.Name.Equals(name));
			if (projectField == null)
			{
				if (_formats == null) _formats = ApiRouter.VisualBlock.GetFieldTypes(autentificator);
				projectField = ApiRouter.ProjectFields.Create(autentificator,
					new ProjectField
					{
						Name = name,
						PlaceholderText = name,
						ProjectFieldFormat = _formats.FirstOrDefault(f => f.Name.Equals("Text"))
					});
			}
			if (_random == null) _random = new Random();
			var tagNamingRule = new Regex("[^a-яA-Яa-zA-Z0-9_]");
			return new VisualBlockField
			{
				IsRequired = bool.Parse(Table.GetValue(header, row, "Required")),
				Tag = tagNamingRule.Replace(Table.GetValue(header, row, "Tag"), "_") + $"_{_random.Next(0, 1000000)}",
				Width = int.Parse(Table.GetValue(header, row, "Width")),
				ProjectField = projectField
			};
		}
	}
}
