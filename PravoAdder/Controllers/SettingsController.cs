using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using PravoAdder.Domain;
using PravoAdder.Helpers;

namespace PravoAdder.Controllers
{
	public class SettingsController
	{
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
						property.SetValue(settingsObject, "prod.xlsx");
						continue;
					case "MaxDegreeOfParallelism":
						property.SetValue(settingsObject, 2);
						continue;
					case "StartRow":
						property.SetValue(settingsObject, 1);
						continue;
				}
#endif
				var ignoreAttibute = (SettingsIgnoreAttribute) property.GetCustomAttributes(typeof(SettingsIgnoreAttribute))
					.FirstOrDefault();
				if (ignoreAttibute != null && ignoreAttibute.Ignore) continue;

				var nameAttribute = (DisplayNameAttribute) property.GetCustomAttributes(typeof(DisplayNameAttribute))
					.FirstOrDefault();
				var displayName = nameAttribute != null ? nameAttribute.DisplayName : property.Name;
				var value = property.GetValue(settingsObject);

				if (!IsEmptyValue(property.PropertyType, value) && property.PropertyType != typeof(bool)) continue;

				var propertyValue = LoadValue(displayName, property.PropertyType, ',');
				property.SetValue(settingsObject, propertyValue);
			}
			if (additionalSettings != null)
				settingsObject.AdditionalSettings = new Dictionary<string, dynamic>(additionalSettings);

			settingsObject.Save(configFilename);
			return settingsObject;
		}

		private static bool IsEmptyValue(Type type, object value)
		{
			var defaultValue = type.IsValueType ? Activator.CreateInstance(type).ToString() : null;
			if (value == null) return true;

			return !string.IsNullOrEmpty(value.ToString()) && value.ToString() == defaultValue;
		}

		private static dynamic LoadValue(string message, Type type, char separator)
		{
			while (true)
			{
                var additionalMessage = type == typeof(bool) ? "(y/n)" : "";
				Console.WriteLine($"{message}{additionalMessage}: ");
				var data = Console.ReadLine();

			    if (type == typeof(bool)) return data == "y";

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