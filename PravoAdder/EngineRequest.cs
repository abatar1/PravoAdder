using System;
using PravoAdder.Api.Domain;
using PravoAdder.Domain;
using PravoAdder.Wrappers;

namespace PravoAdder
{
	public class EngineRequest : EngineResponse
	{
		public Settings Settings { get; set; }
		public ApiEnviroment ApiEnviroment { get; set; }
		public ApplicationArguments AppArgs { get; set; }
		public Counter Counter { get; set; } = new Counter();
		public BlockReaderWrapper BlockReader { get; set; }
		public Row ExcelRow { get; set; }		
		public DateTime Date { get; set; }
		public int Count { get; set; }
		public bool IsUpdate { get; set; }
		public Task Task { get; set; }
		public ExtendentParticipant Participant { get; set; }
	}
}

