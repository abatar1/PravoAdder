using System;
using PravoAdder.Domain;

namespace PravoAdder.Processors
{
	public interface IProcessor
	{
		ApplicationArguments ApplicationArguments { get; }
		Func<EngineRequest, EngineResponse> Processor { get; }
		void Run();
	}
}
