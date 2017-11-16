using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PravoAdder.Helpers
{
	public static class StringHelper
	{
		public static string SliceSpaceIfMore(this string item, int threshold)
		{
			if (item == null || item.Length <= threshold) return item;

			var lastSpacePosition = item.LastIndexOf(' ', threshold);
			return $"{item.Remove(lastSpacePosition)}";
		}

		public static DateTime FormatDate(this string item)
		{
			var rawDate = item.Replace("UTC", "").Trim();
			DateTime.TryParseExact(rawDate,
				new[] {"MM/dd/yyyy hh:mm tt", "MM/dd/yyyy h:mm tt", "MM.dd.yy hh:mm", "M.dd.yy", "M.d.yy", "MM.d.yy", "MM.dd.yy"},
				CultureInfo.InvariantCulture,
				DateTimeStyles.None, out var date);
			return date;
		}

		private static Random _random;
		public static string ToTag(this string item)
		{
			if (_random == null) _random = new Random();
			var tagNamingRule = new Regex("[^a-яA-Яa-zA-Z0-9_]");

			return tagNamingRule.Replace(item, "_") + $"_{_random.Next(0, 1000000)}";
		}

		public static string[] GetCommandsFromString(this string line)
		{
			if (line == null) return null;

			var words = line.Split(' ');
			var commands = new List<string>();

			for (var i = 0; i < words.Length; i++)
			{
				commands.Add(words[i]);
				if (words[i].StartsWith("-")) continue;

				var count = 1;
				while (true)
				{
					if (i + count >= words.Length || words[i + count].StartsWith("-"))
					{
						i += count - 1;
						break;
					}
					commands[commands.Count - 1] += $" {words[i + count]}";
					count += 1;
				}
			}

			return commands.ToArray();
		}
	}
}
