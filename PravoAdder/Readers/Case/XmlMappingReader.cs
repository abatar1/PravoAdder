﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Fclp.Internals.Extensions;
using Newtonsoft.Json.Linq;
using PravoAdder.Domain;
using PravoAdder.Helpers;

namespace PravoAdder.Readers
{
	public class XmlMappingReader : TemplateTableReader
	{
		private Dictionary<string, HashSet<XmlAddress>> _matching;

		public override Table Read(Settings settings)
		{
			var sourceInfo = GetFileInfo(settings.SourceName, ".xml");			
			var mappingInfo = GetFileInfo(settings.XmlMappingPath, ".json");
			if (sourceInfo == null || mappingInfo == null) throw new FileNotFoundException();

			var xdoc = XDocument.Load(sourceInfo.Name);
			var mapping = (JArray) JObject.Parse(File.ReadAllText(mappingInfo.Name)).GetValue("Blocks");
			_matching = GetMatching(mapping);					

			var table = new List<Dictionary<int, FieldAddress>>();

			foreach (var p in xdoc.Elements("Cases").Elements().Select((value, count) => new { Value = value, Count = count + 1}))
			{
				if (p.Count < settings.RowNum) continue;
				
				var project = p.Value;		
				var header = ReadHeaderFromX(project);
				if (header == null) continue;

				var readingMode = settings.FieldReadingMode;

				var row = new Dictionary<int, FieldAddress>();
				row.AddRange(AdaptHeader(header, readingMode));		

				var attributes = project.Elements("Attributes")
					.Elements()
					.Skip(1)
					.Elements();
				row.AddRange(AdaptXElement(attributes, readingMode));

				var instances = project.Elements("Instances")
					.Elements()
					.Elements();
				row.AddRange(AdaptXElement(instances, readingMode));

				var activities = project.Elements("Activities")
					.Elements()
					.Elements();
				row.AddRange(AdaptXElement(activities, readingMode));

				var recoveries = project.Elements("Recoveries")
					.Elements()
					.Elements();
				row.AddRange(AdaptXElement(recoveries, readingMode));

				var payments = project.Elements("Payments")
					.Elements()
					.Elements();
				row.AddRange(AdaptXElement(payments, readingMode));

				table.Add(row);
			}

			var infos = _matching
				.SelectMany(match => match.Value)
				.ToDictionary(value => value.Count, value => value.ToFieldAddress());

			return new Table(table.Select(row => new Row(row)), new Row(infos));
		}

		private Dictionary<int, FieldAddress> AdaptXElement(IEnumerable<XElement> xElements, FieldReadingMode readingMode)
		{
			if (xElements == null) return null;

			var row = new Dictionary<int, FieldAddress>();
			var repeatCount = new Dictionary<string, int>();

			foreach (var xElement in xElements)
			{
				var xmlTag = xElement.Elements("Tag").First().Value;
				if (!_matching.ContainsKey(xmlTag)) continue;

				var value = new FieldAddress(xElement.Elements("Value").First().Value, readingMode);
				foreach (var xmlAddress in _matching[xmlTag].Where(x => x.RepeatBlockNumber <= 1))
				{				
					if (row.ContainsKey(xmlAddress.Count))
					{
						if (!repeatCount.ContainsKey(xmlTag)) repeatCount.Add(xmlTag, 0);
						repeatCount[xmlTag] += 1;

						var blockNumber = _matching[xmlTag]
							.Count(x => x.NameEquals(xmlAddress));
						if (blockNumber == 1)
						{
							_matching[xmlTag] = _matching[xmlTag]
								.Select(x => x.NameEquals(xmlAddress) ? x.ToRepeatBlock(x.Count, blockNumber) : x)
								.ToHashSet();
						}
						if (repeatCount[xmlTag] >= blockNumber)
						{
							_matching[xmlTag].Add(xmlAddress.ToRepeatBlock(XmlAddress.MaxCount + 1, blockNumber + 1));
							row.Add(XmlAddress.MaxCount, value);
							break;
						}
						row.Add(_matching[xmlTag].First(x => x.RepeatBlockNumber == repeatCount[xmlTag] + 1).Count, value);
						break;
					}
					row.Add(xmlAddress.Count, value);
				}
			}
			return row;
		}

		private static Dictionary<string, HashSet<XmlAddress>> GetMatching(JArray mapping)
		{
			var matching = new Dictionary<string, HashSet<XmlAddress>>();
			var count = 1;
			foreach (var block in mapping)
			{
				var blockName = (string) block["Name"];
				foreach (var field in (JArray) block["Fields"])
				{
					var fieldName = (string) field["Name"];
					var fieldTag = (string) field["Tag"];
					if (!matching.ContainsKey(fieldTag))
					{
						var address = new HashSet<XmlAddress> {new XmlAddress(blockName, fieldName, count)};
						matching.Add(fieldTag, address);
						count += 1;
					}
					else
					{
						if (matching[fieldTag].Add(new XmlAddress(blockName, fieldName, count)))
						{
							count += 1;
						}
					}
				}				
			}
			return matching;
		}

		private Dictionary<int, FieldAddress> AdaptHeader(HeaderBlockInfo headerInfo, FieldReadingMode readingMode)
		{
			var row = new Dictionary<int, FieldAddress>();
			foreach (var property in headerInfo.GetType().GetProperties())
			{
				var value = property.GetValue(headerInfo);
				if (value == null) continue;

				var key = $"Sys_{property.Name}";
				_matching[key].ForEach(address =>
				{
					row.Add(address.Count, new FieldAddress(value.ToString(), readingMode));
				});			
			}
			return row;
		}

		private static HeaderBlockInfo ReadHeaderFromX(XElement project)
		{
			var caseInfoAttribute = project.DescendantsAndSelf("Attributes")
				.Elements()
				.FirstOrDefault();
			if (caseInfoAttribute == null) return null;

			var projectType = caseInfoAttribute.DescendantsAndSelf("CaseType")
				.FirstOrDefault()
				?.DescendantsAndSelf("Name")
				.FirstOrDefault()
				?.Value;
			var responsible = caseInfoAttribute.DescendantsAndSelf("ResponseUser")
				.FirstOrDefault()
				?.DescendantsAndSelf("Name")
				.FirstOrDefault()
				?.Value;
			var projectFolder = caseInfoAttribute.DescendantsAndSelf("Filial")
				.FirstOrDefault()
				?.DescendantsAndSelf("Name")
				.FirstOrDefault()
				?.Value;
			var projectName = caseInfoAttribute.DescendantsAndSelf("Name")
				.FirstOrDefault()
				?.Value;

			return new HeaderBlockInfo
			{
				ProjectFolder = projectFolder,
				Name = projectName,
				ProjectType = projectType,
				Responsible = responsible
			};
		}
	}
}
