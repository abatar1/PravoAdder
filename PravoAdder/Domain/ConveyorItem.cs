using System;

namespace PravoAdder.Domain
{
	[Serializable]
	public class ConveyorItem
	{
		public Func<EngineMessage, EngineMessage> Processor { get; set; }
		public EngineMessage Message { get; set; }
		public int Depth { get; set; }
	}
}