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
    }
}
