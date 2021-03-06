﻿using System;

namespace PravoAdder.Domain
{
	public class FieldNameAttribute : Attribute
	{
		public FieldNameAttribute(params string[] names)
		{
			FieldNames = names;
		}

		public string[] FieldNames { get; set; }
	}
}
