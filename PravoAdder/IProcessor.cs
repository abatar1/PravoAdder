using System;

namespace PravoAdder
{
	public interface IProcessor
	{
		Func<EngineRequest, EngineResponse> Processor { get; }
		void Run();
	}
}
