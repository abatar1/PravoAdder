using System;

namespace PravoAdder.Processors
{
	public interface IProcessor
	{
		string ConfigFilename { get; }
		Func<EngineRequest, EngineResponse> Processor { get; }
		void Run();
	}
}
