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
    }
}
