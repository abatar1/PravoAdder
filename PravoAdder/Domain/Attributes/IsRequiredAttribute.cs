using System;

namespace PravoAdder.Domain.Attributes
{
	public class IsRequiredAttribute : Attribute
	{
		public IsRequiredAttribute(bool isRequired)
		{
			IsRequiredValue = isRequired;
		}

		public bool IsRequiredValue { get; set; }
	}
}
