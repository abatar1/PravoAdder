using System;
using System.Collections.Generic;
using PravoAdder.Domain;

namespace PravoAdder.Processors
{
	public static class GroupedProcessors
	{
		public static List<Func<EngineMessage, EngineMessage>> LoadWithTable = new List<Func<EngineMessage, EngineMessage>>
		{
			SingleProcessors.Core.LoadTable,
			SingleProcessors.Core.InitializeApp
		};

		public static List<Func<EngineMessage, EngineMessage>> LoadWithoutTable = new List<Func<EngineMessage, EngineMessage>>
		{
			SingleProcessors.Core.InitializeApp
		};

		public static List<Func<EngineMessage, EngineMessage>> LoadWithFormattedTable(Func<EngineMessage, EngineMessage> formatter)
		{
			return new List<Func<EngineMessage, EngineMessage>>
			{
				formatter,
				SingleProcessors.Core.LoadTable,
				SingleProcessors.Core.InitializeApp
			};
		}
	}
}
