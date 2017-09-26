using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using PravoAdder.Domain;
using PravoAdder.Helpers;

namespace PravoAdder.Wrappers
{
    public class SettingsWrapper
    {
	    private static ReaderMode _blockReadingMode = ReaderMode.All;

        public Settings LoadSettingsFromConsole(ApplicationArguments applicationArguments, Dictionary<string, dynamic> additionalSettings = null)
        {
            Console.WriteLine("Reading config files...");
            var settingsObject = SettingsHelper.Read(applicationArguments.ConfigFilename);

            foreach (var property in settingsObject.GetType().GetProperties())
            {
	            var processTypeAttribute = LoadAttribute<ProcessTypeAttribute>(property);
	            if (!processTypeAttribute.ProcessTypes.Contains(applicationArguments.ProcessType) &&
	                !processTypeAttribute.ProcessTypes.Contains(ProcessType.All)) continue;

	            var readingTypeAttribute = LoadAttribute<ReadingTypeAttribute>(property);
	            if (readingTypeAttribute != null &&
	                !readingTypeAttribute.ReadingTypes.Contains(_blockReadingMode)) continue;

#if DEBUG
				switch (property.Name)
				{
					case "Password":
						property.SetValue(settingsObject, "123123");
						continue;
					case "Overwrite":
						property.SetValue(settingsObject, true);
						continue;
					case "SourceFileName":
						property.SetValue(settingsObject, "prod.xml");
						continue;
					case "MaxDegreeOfParallelism":
						property.SetValue(settingsObject, 1);
						continue;
					case "StartRow":
						property.SetValue(settingsObject, 1);
						continue;
				}
#endif
	            	          
	            var ignoreAttibute = LoadAttribute<IgnoreAttribute>(property);
                if (ignoreAttibute != null && ignoreAttibute.Ignore) continue;
              
                var value = property.GetValue(settingsObject);
                if (!IsEmptyValue(property.PropertyType, value) && property.PropertyType != typeof(bool)) continue;

	            var nameAttribute = LoadAttribute<DisplayNameAttribute>(property);
				var displayName = nameAttribute != null ? nameAttribute.DisplayName : property.Name;

				var propertyValue = LoadValue(displayName, property.PropertyType, ',');
                property.SetValue(settingsObject, propertyValue);
			}
	        if (additionalSettings != null)
	        {
				settingsObject.AdditionalSettings = new Dictionary<string, dynamic>(additionalSettings);
			}
               
            settingsObject.Save(applicationArguments.ConfigFilename);
            return settingsObject;
		}

	    private static T LoadAttribute<T>(MemberInfo property) where T : Attribute
	    {
		    return (T) property
			    .GetCustomAttributes(typeof(T))
			    .FirstOrDefault();
		}

		private static bool IsEmptyValue(Type type, object value)
        {
            var defaultValue = type.IsValueType ? Activator.CreateInstance(type).ToString() : null;
            if (value == null) return true;

            return !string.IsNullOrEmpty(value.ToString()) && value.ToString() == defaultValue;
        }

        private static dynamic LoadValue(string message, Type type, char separator)
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
		                if (Enum.TryParse(data, out ReaderMode result))
		                {
			                if (type == typeof(ReaderMode)) _blockReadingMode = result;
							return result;
		                }
	                }
                    return Convert.ChangeType(data, type);
                }
                Console.WriteLine($"Wrong {message}!");
            }
        }
	}
}