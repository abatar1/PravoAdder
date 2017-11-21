using System;
using System.ComponentModel;
using System.Linq;
using PravoAdder.Domain;
using PravoAdder.Helpers;

namespace PravoAdder.Wrappers
{
    public class SettingsWrapper
    {
	    private static ReadingMode _blockReadingMode = ReadingMode.All;

		public Settings LoadSettingsFromConsole(ApplicationArguments applicationArguments)
        {
            Console.WriteLine("Reading config files...");
            var settingsObject = Settings.Read(applicationArguments.ConfigFileName);

            foreach (var property in settingsObject.GetType().GetProperties())
            {
	            var readingTypeAttribute = property.LoadAttribute<ReadingTypeAttribute>();
	            if (readingTypeAttribute != null &&
	                !readingTypeAttribute.ReadingTypes.Contains(_blockReadingMode)) continue;
	            	          
	            var ignoreAttibute = property.LoadAttribute<IgnoreAttribute>();
                if (ignoreAttibute != null && ignoreAttibute.Ignore) continue;
              
                var value = property.GetValue(settingsObject);
                if (!IsEmptyValue(property.PropertyType, value) && property.PropertyType != typeof(bool)) continue;

	            var nameAttribute = property.LoadAttribute<DisplayNameAttribute>();
				var displayName = nameAttribute != null ? nameAttribute.DisplayName : property.Name;

	            var requiredAttribute = property.LoadAttribute<IsRequiredAttribute>();
	            var isRequired = requiredAttribute.IsRequiredValue;

				var propertyValue = LoadValue(displayName, property.PropertyType, ',', isRequired);
                property.SetValue(settingsObject, propertyValue);
			}
               
            settingsObject.Save(applicationArguments.ConfigFileName);
            return settingsObject;
		}

		private static bool IsEmptyValue(Type type, object value)
        {
            var defaultValue = type.IsValueType ? Activator.CreateInstance(type).ToString() : null;
            if (value == null) return true;

            return !string.IsNullOrEmpty(value.ToString()) && value.ToString() == defaultValue;
        }

        private static dynamic LoadValue(string message, Type type, char separator, bool isRequired)
        {
	        if (type == null) return null;
		        
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
							.Select(d => Convert.ChangeType(d, type.GetElementType())?.ToString())
							.ToArray();
					}
	                if (type.IsEnum)
	                {
		                if (Enum.TryParse(data, out ReadingMode result))
		                {
			                if (type == typeof(ReadingMode)) _blockReadingMode = result;
							return result;
		                }
						Console.WriteLine("Wrong enum item passed.");
	                }
	                else if (data == "max")
	                {
		                return int.MaxValue;
	                }
                    return Convert.ChangeType(data, type);
                }
	            if (!isRequired)
	            {
		            return null;
	            }
				Console.WriteLine($"Wrong {message}!");
            }       
        }
	}
}