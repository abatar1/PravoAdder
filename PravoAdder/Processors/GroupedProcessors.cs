using System;
using System.Collections.Generic;
using PravoAdder.Domain;

namespace PravoAdder.Processors
{
	public static class GroupedProcessors
	{
		public static List<Func<EngineMessage, EngineMessage>> LoadWithTable = new List<Func<EngineMessage, EngineMessage>>()
		{
			SingleProcessors.LoadSettings,
			SingleProcessors.LoadTable,
			SingleProcessors.InitializeApp
		};

		public static List<Func<EngineMessage, EngineMessage>> LoadWithoutTable = new List<Func<EngineMessage, EngineMessage>>()
		{
			SingleProcessors.LoadSettings,
			SingleProcessors.InitializeApp
		};
	}
}
