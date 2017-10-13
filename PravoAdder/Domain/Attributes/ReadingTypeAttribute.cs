using System;

namespace PravoAdder.Domain.Attributes
{
	public class ReadingTypeAttribute : Attribute
	{
		public ReadingTypeAttribute(params ReaderMode[] readingTypes)
		{
			ReadingTypes = readingTypes;
		}

		public ReaderMode[] ReadingTypes { get; set; }
	}
}
