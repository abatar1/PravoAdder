using System;
using System.Threading.Tasks;
using PravoAdder.Domain;
using PravoAdder.Readers;
using PravoAdder.Wrappers;

namespace PravoAdder.Processors
{
	public class CoreProcessors
	{
		public Func<EngineMessage, EngineMessage> LoadSettings = message =>
		{
			var settingsController = new SettingsWrapper();
			return new EngineMessage
			{
				Settings = settingsController.LoadSettingsFromConsole(message.Args),
				ParallelOptions = new ParallelOptions { MaxDegreeOfParallelism = message.Args.ParallelOptions }
			};
		};

		public Func<EngineMessage, EngineMessage> LoadTable = message =>
		{
			TableEnviroment.Initialize(message.Args, message.Settings);
			return new EngineMessage { Table = TableEnviroment.Table, Settings = message.Settings };
		};

		public Func<EngineMessage, EngineMessage> InitializeApp = message =>
		{
			var authenticatorController = new AuthentificatorWrapper(message.Settings, message.Args);
			var authenticator = authenticatorController.Authenticate();
			if (authenticator == null)
			{
				message.IsFinal = true;
				return message;
			}

			var processType = message.Args.ProcessType;
			var processName = Enum.GetName(typeof(ProcessType), processType);
			if (processName == null) return new EngineMessage { IsFinal = true };

			ICreator creator = null;
			if (processName.Contains("Participant")) creator = new ParticipantCreator(authenticator, message.Args.ParticipantType);
			if (processName.Contains("Task")) creator = new TaskCreator(authenticator);
			if (processName.Contains("ProjectField")) creator = new ProjectFieldCreator(authenticator);
			if (processName.Contains("VisualBlockLine")) creator = new VisualBlockLineCreator(authenticator);
			if (processName.Contains("Event")) creator = new EventCreator(authenticator);
			if (processName.Contains("BillingRules")) creator = new BillingRulesCreator(authenticator);

			return new EngineMessage
			{
				Authenticator = authenticator,
				CaseBuilder = new CaseBuilder(message.Table, message.Settings, authenticator),
				ApiEnviroment = new ApiEnviroment(authenticator),
				Counter = new Counter(),
				Creator = creator
			};
		};

		public Func<EngineMessage, EngineMessage> ProcessCount = message =>
		{
			if (message.Item == null) return message;
			message.Counter.ProcessCount(message.Count, message.Total, message.Args.RowNum, message.Item, 70);
			return message;
		};
	}
}
