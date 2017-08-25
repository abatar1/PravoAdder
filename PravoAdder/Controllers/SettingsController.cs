using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using PravoAdder.Domain;
using PravoAdder.Helpers;
using PravoAdder.Readers;

namespace PravoAdder.Controllers
{
	public class SettingsController
	{
		public SettingsController(TextWriter writer)
		{
			Console.SetOut(writer);			
		}

		public Settings LoadSettings(string configFilename, Dictionary<string, dynamic> additionalSettings = null)
		{
			Console.WriteLine("Reading config files...");
			var settingsObject = SettingsHelper.Read(configFilename);

			foreach (var property in settingsObject.GetType().GetProperties())
			{
#if DEBUG
				switch (property.Name)
				{
					case "Password":
						property.SetValue(settingsObject, "123123");
						continue;
					case "Overwrite":
						property.SetValue(settingsObject, false);
						continue;
					case "ExcelFileName":
						property.SetValue(settingsObject, "test3.xlsx");
						continue;
					case "DataRowPosition":
						property.SetValue(settingsObject, 4);
						continue;
				}
#endif
				var ignoreAttibute = (SettingsIgnoreAttribute) property.GetCustomAttributes(typeof(SettingsIgnoreAttribute))
					.FirstOrDefault();
				if (ignoreAttibute == null || ignoreAttibute.Ignore) continue;

				var nameAttribute = (DisplayNameAttribute)property.GetCustomAttributes(typeof(DisplayNameAttribute))
					.FirstOrDefault();
				var displayName = nameAttribute != null ? nameAttribute.DisplayName : property.Name;
				var value = property.GetValue(settingsObject);

				if (string.IsNullOrEmpty(value?.ToString()) || property.PropertyType == typeof(bool))
				{
					var propertyValue = LoadValue(displayName, property.PropertyType, ',');
					property.SetValue(settingsObject, propertyValue);
				}
			}

			if (additionalSettings != null) settingsObject.AdditionalSettings = new Dictionary<string, dynamic>(additionalSettings);

			settingsObject.Save(configFilename);
			return settingsObject;
		}

		private static dynamic LoadValue(string message, Type type, char separator)
		{
			while (true)
			{
				Console.WriteLine($"{message}: ");
				var data = Console.ReadLine();
				if (!string.IsNullOrEmpty(data))
				{
					if (type.IsArray)
					{
						return data
							.Split(separator)
							.Select(d => Convert.ChangeType(d, type.GetElementType()).ToString())
							.ToArray();
					}
					return Convert.ChangeType(data, type);
				}
				Console.WriteLine($"Wrong {message}!");
			}
		}
	}
}
