using System;
using PravoAdder.Domain;

namespace PravoAdder.Processors
{
	public class ContactProcessor : IProcessor
	{
		public ContactProcessor(ApplicationArguments applicationArguments, Func<EngineRequest, EngineResponse> processor)
		{
			ApplicationArguments = applicationArguments;
			Processor = processor;
		}

		public ApplicationArguments ApplicationArguments { get; }
		public Func<EngineRequest, EngineResponse> Processor { get; }
		public void Run()
		{
			throw new NotImplementedException();
		}
	}
}
