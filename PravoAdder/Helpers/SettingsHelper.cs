using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PravoAdder.Domain;

namespace PravoAdder.Helpers
{
    public static class SettingsHelper
    {
        public static Settings Read(string filePath)
        {
            var info = new FileInfo(filePath);
            if (!info.Exists) File.Create(info.FullName).Dispose();

            var rawJson = File.ReadAllText(filePath);
            return string.IsNullOrEmpty(rawJson) ? new Settings() : JsonConvert.DeserializeObject<Settings>(rawJson);
        }

        public static void Save(this Settings settings, string settingsFilePath)
        {
            var jsonSettings = JsonConvert.SerializeObject(settings, Formatting.Indented);

            var info = new FileInfo(settingsFilePath);

            if (!info.Exists) File.Create(info.FullName).Dispose();
            File.WriteAllText(settingsFilePath, jsonSettings);
        }

	    public static string[] GetCommandsFromString(string line)
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