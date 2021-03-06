﻿using System;
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
	public class VisualBlockLineCreator : Creator
	{
		public VisualBlockModel VisualBlock { get; set; }
		public VisualBlockLineModel ConstructedLineModel { get; set; }

		public override ICreatable Create(Table table, Row row, DatabaseEntityItem item = null)
		{
			var visualBlockName = table.GetValue(row, "Data Block");

			string repeatingFieldValue;
			try
			{
				repeatingFieldValue = table.GetValue(row, "Repeating Block");
			}
			catch (Exception)
			{
				repeatingFieldValue = null;
			}
			var isRepeatingBlock = bool.Parse(repeatingFieldValue ?? "false");

			var puttingVisualBlock = new VisualBlockModel
			{
				NameInConstructor = visualBlockName,
				IsRepeatable = isRepeatingBlock,
				Lines = new List<VisualBlockLineModel>()
			};
			VisualBlock = VisualBlockRepository.GetOrCreate(HttpAuthenticator, visualBlockName, puttingVisualBlock);

			var newField = GetVisualBlockField(HttpAuthenticator, table, row);
			if (newField == null) return null;

			VisualBlockLineModel lineModel;
			if (ConstructedLineModel != null)
			{
				lineModel = ConstructedLineModel.CloneJson();
				lineModel.Fields.Add(newField);
			}
			else
			{
				var rowNamingRule = new Regex("[^a-яA-Яa-zA-Z ]");
				var rowType = rowNamingRule.Replace(table.GetValue(row, "Row"), "").Trim();
				var lineType = _lineTypes.First(t => t.Name.Equals(rowType, StringComparison.InvariantCultureIgnoreCase));
				lineModel = new VisualBlockLineModel
				{
					LineType = lineType,
					Fields = new List<VisualBlockFieldModel>(),
					Order = VisualBlock.Lines.Count, 
				};
				lineModel.Fields.Add(newField);
			}

			return lineModel;
		}

		private static List<ProjectFieldFormat> _formats;
		private static List<LineType> _lineTypes;

		private static VisualBlockFieldModel GetVisualBlockField(HttpAuthenticator autenticator, Table table, Row row)
		{
			var fieldName = table.GetValue(row, "Field Name")?.SliceSpaceIfMore(256);
			if (string.IsNullOrEmpty(fieldName)) return null;
			
			var newProjectField = new ProjectField
			{
				Name = fieldName,
				PlaceholderText = fieldName,
				ProjectFieldFormat = _formats.FirstOrDefault(f => f.Name.Equals("Text"))
			};
			var projectField = ProjectFieldRepository.GetOrCreate(autenticator, fieldName, newProjectField);
			
			return new VisualBlockFieldModel
			{
				IsRequired = bool.Parse(table.GetValue(row, "Required")),
				Tag = table.GetValue(row, "Tag").ToTag(),
				Width = int.Parse(table.GetValue(row, "Width")),
				ProjectField = projectField
			};
		}

		public VisualBlockLineCreator(HttpAuthenticator httpAuthenticator, Settings settings) : base(httpAuthenticator, settings)
		{
			_formats = ApiRouter.Bootstrap.GetFieldTypes(httpAuthenticator);
			_lineTypes = ApiRouter.Bootstrap.GetLineTypes(HttpAuthenticator);
		}
	}
}
