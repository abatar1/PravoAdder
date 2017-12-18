using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace PravoAdder.Domain
{
	public class InstanceEnviroment
	{
		public ICollection<Instance> Instances { get; set; }
		public Instance CurrentInstance { get; set; }

		private static string _filePath;

		public InstanceEnviroment()
		{
			Instances = new List<Instance>();
			CurrentInstance = new Instance();
		}

		public static InstanceEnviroment Read(string filePath)
		{
			_filePath = filePath;

			var info = new FileInfo(filePath);
			if (!info.Exists) File.Create(info.FullName).Dispose();

			var rawJson = File.ReadAllText(filePath);

			return string.IsNullOrEmpty(rawJson)
				? new InstanceEnviroment()
				: JsonConvert.DeserializeObject<InstanceEnviroment>(rawJson);
		}

		public void Save()
		{
			if (string.IsNullOrEmpty(_filePath)) throw new ArgumentException("You need to Read() InstanceEnviroment before Save().");

			var jsonSettings = JsonConvert.SerializeObject(this, Formatting.Indented);

			var info = new FileInfo(_filePath);

			if (!info.Exists) File.Create(info.FullName).Dispose();
			File.WriteAllText(_filePath, jsonSettings);
		}
	}
}
