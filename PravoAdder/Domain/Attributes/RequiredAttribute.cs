using System;

namespace PravoAdder.Domain
{
	public class RequiredAttribute : Attribute
	{
		public RequiredAttribute(bool isRequired = true)
		{
			IsRequiredValue = isRequired;
		}

		public bool IsRequiredValue { get; set; }
	}
}
