using System;
using System.Collections.Generic;
using System.Linq;
using PravoAdder.Domain;

namespace PravoAdder.Processors
{
	public class ProcessConveyor
	{
		private List<ConveyorItem> Conveyor { get; }
		private EngineMessage FirstMessage { get; }

		public ProcessConveyor(ApplicationArguments arguments)
		{
			FirstMessage = new EngineMessage { ApplicationArguments = arguments };
			Conveyor = new List<ConveyorItem>();
		}

		public void Add(Func<EngineMessage, EngineMessage> processor, int depth = 0)
		{
			Conveyor.Add(new ConveyorItem { Depth = depth, Processor = processor, Message = new EngineMessage()});		
		}

		public void AddRange(List<Func<EngineMessage, EngineMessage>> processors)
		{
			foreach (var processor in processors)
			{
				Add(processor);
			}
		}

		public void Run()
		{
			var conveyorСounter = 0;
			var messageConveyor = FirstMessage;
			foreach (var conveyorIter in Conveyor)
			{
				if (conveyorСounter == 0 || conveyorIter.Depth == 0)
				{
					messageConveyor.Concat(conveyorIter.Message);
					var responseMessage = conveyorIter.Processor(messageConveyor);
					messageConveyor.Concat(responseMessage);					
				}
				else
				{				
					var startIndex = conveyorСounter + 1;
					var parentIndex = startIndex + Conveyor
						.Skip(startIndex)
						.ToList()
						.FindIndex(iter => iter.Depth != conveyorIter.Depth);
					if (Conveyor[parentIndex].Message.Child == null)
					{
						Conveyor[parentIndex].Message.Child = new List<ConveyorItem>();
					}
					conveyorIter.Message.Concat(messageConveyor);
					Conveyor[parentIndex].Message.Child.Add(conveyorIter);				
				}
				conveyorСounter += 1;
			}
		}
	}
}
