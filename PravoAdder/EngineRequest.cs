﻿using System;
using System.Collections.Generic;
using PravoAdder.Domain;
using PravoAdder.Wrappers;

namespace PravoAdder
{
	public class EngineRequest : EngineResponse
	{
		public Settings Settings { get; set; }
		public ApiEnviroment ApiEnviroment { get; set; }
		public Counter Counter { get; set; } = new Counter();
		public BlockReaderWrapper BlockReader { get; set; }
		public IDictionary<int, string> ExcelRow { get; set; }		
		public int Index { get; set; }
		public DateTime Date { get; set; }
	}
}
