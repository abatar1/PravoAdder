using System;

namespace PravoAdder.Domain.Attributes
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
