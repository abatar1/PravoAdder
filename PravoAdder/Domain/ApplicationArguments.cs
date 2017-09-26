using System;

namespace PravoAdder.Domain
{
	public class ApplicationArguments
	{
		public string ConfigFilename { get; set; }
		public ProcessType ProcessType { get; set; }
	}

	[Flags]
	public enum ProcessType
	{
		Migration,
		Syncronization,
		Cleaning,
		Test,
		All
	}
}
