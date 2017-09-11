using System;

namespace PravoAdder.Domain
{
	public class FieldNameAttribute : Attribute
	{
		public FieldNameAttribute(string name)
		{
			FieldName = name;
		}

		public string FieldName { get; set; }
	}
}
