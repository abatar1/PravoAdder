using System;
using System.Linq;
using System.Reflection;

namespace PravoAdder.Helpers
{
	public static class AttributeHelper
	{
		public static T LoadAttribute<T>(this MemberInfo property) where T : Attribute
		{
			return (T)property
				.GetCustomAttributes(typeof(T))
				.FirstOrDefault();
		}
	}
}
