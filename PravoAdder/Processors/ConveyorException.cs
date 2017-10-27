using System;

namespace PravoAdder.Processors
{
	[Serializable]
	public class ConveyorException : Exception
	{
		public ConveyorException(string message) : base(message)
		{
		}
	}
}
