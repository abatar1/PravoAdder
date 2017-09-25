using System;

namespace PravoAdder
{
	public class CleanProcessor : IProcessor
	{
		public CleanProcessor(Func<EngineRequest, EngineResponse> processor)
		{
			Processor = processor;
		}

		public Func<EngineRequest, EngineResponse> Processor { get; }

		public void Run()
		{
			throw new NotImplementedException();
		}
	}
}
