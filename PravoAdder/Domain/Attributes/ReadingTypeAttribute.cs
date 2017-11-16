using System;

namespace PravoAdder.Domain.Attributes
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
