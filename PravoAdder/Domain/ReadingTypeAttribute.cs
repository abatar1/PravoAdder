using System;

namespace PravoAdder.Domain
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
