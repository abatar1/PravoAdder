using System;

namespace PravoAdder.Domain
{
	public class DefaultValueAttribute : Attribute
	{
		public DefaultValueAttribute(object value)
		{
			DefaultValue = value;
		}

		public object DefaultValue { get; }
	}
}
