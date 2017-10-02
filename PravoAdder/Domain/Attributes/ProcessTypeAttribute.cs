using System;

namespace PravoAdder.Domain
{
	public class ProcessTypeAttribute : Attribute
	{
		public ProcessTypeAttribute(params ProcessType[] processTypes)
		{
			ProcessTypes = processTypes;
		}

		public ProcessType[] ProcessTypes { get; set; }
	}
}
