using System;
using NLog;
using PravoAdder.Domain;

namespace PravoAdder
{
	public class Engine
	{
		private static Settings _settings;
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		public Engine(Settings settings)
		{
			_settings = settings;
		}

		public void Run()
		{
			var conveyor = new ProcessConveyor(_settings);
			var isSuccess = conveyor.Create().Run();
			if (isSuccess) Logger.Info($"{DateTime.Now} | {_settings.ProcessType} successfully processed. Press any key to continue.");

			Console.ReadKey();
		}
	}
}
