using System.IO;
using Newtonsoft.Json;
using PravoAdder.Domain;

namespace PravoAdder.Reader
{
    public static class SettingsReader
    {
        public static Settings Read(string filePath)
        {
            var rawJson = File.ReadAllText(filePath);

            return JsonConvert.DeserializeObject<Settings>(rawJson);
        }

        public static void Save(this Settings settings, string settingsFileName)
        {
            var jsonSettings = JsonConvert.SerializeObject(settings);

            File.WriteAllText(settingsFileName, jsonSettings);            
        }
    }
}
