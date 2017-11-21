using System;

namespace PravoAdder.Domain
{
	public class ReadingTypeAttribute : Attribute
	{
		public ReadingTypeAttribute(params ReadingMode[] readingTypes)
		{
			ReadingTypes = readingTypes;
		}

		public ReadingMode[] ReadingTypes { get; set; }
	}
}
