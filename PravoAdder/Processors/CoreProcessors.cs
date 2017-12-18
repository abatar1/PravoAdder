using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NLog;
using PravoAdder.Domain;
using PravoAdder.Helpers;
using PravoAdder.Readers;
using PravoAdder.Wrappers;

namespace PravoAdder.Processors
{
	public class CoreProcessors
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		public Func<EngineMessage, EngineMessage> LoadTable = message =>
		{
			Logger.Info($"Reading {message.Settings.SourceName} file.");
			message.Table = TableEnviroment.Create(message.Settings.SourceName, message.Settings);
			return message;
		};

		public Func<EngineMessage, EngineMessage> InitializeApp = message =>
		{
			var authenticatorController = new AuthentificatorWrapper(message.Settings, TimeSpan.FromMinutes(2), 5);
			var authenticator = authenticatorController.Authenticate();
			if (authenticator == null)
			{
				message.IsFinal = true;
				return message;
			}

			var processType = message.Settings.ProcessType;
			var processName = ProcessTypes.GetByName(processType).Name;
			if (processName == null) return new EngineMessage { IsFinal = true };

			var creators = Assembly
				.GetAssembly(typeof(Creator))
				.GetTypes()
				.Where(t => t.IsSubclassOf(typeof(Creator)))
				.ToDictionary(key => key.Name, value => (Creator)Activator.CreateInstance(value, authenticator, message.Settings));

			return new EngineMessage
			{
				Authenticator = authenticator,
				CaseBuilder = new CaseBuilder(message.Table, message.Settings, authenticator),
				ApiEnviroment = new ApiEnviroment(authenticator),
				Counter = new Counter(),
				Creators = creators,
				ParallelOptions = new ParallelOptions {MaxDegreeOfParallelism = message.Settings.ParallelOptions}
			};
		};

		public Func<EngineMessage, EngineMessage> ProcessCount = message =>
		{
			if (message.Item == null) return message;
			message.Counter.ProcessCount(message.Count, message.Total, message.Settings.RowNum, message.Item, 70);
			return message;
		};
	}
}
