﻿using System;
using System.Linq;
using System.Threading.Tasks;
using NLog.Config;
using PravoAdder.Domain;
using PravoAdder.Helpers;
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
			var authenticatorController = new AuthentificatorWrapper(message.Args);
			var authenticator = authenticatorController.Authenticate();
			if (authenticator == null)
			{
				message.IsFinal = true;
				return message;
			}

			var processType = message.Args.ProcessType;
			var processName = Enum.GetName(typeof(ProcessType), processType);
			if (processName == null) return new EngineMessage { IsFinal = true };

			var subjectType = processName.SplitCamelCase()[0];
			var creatorType = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => typeof(Creator).IsAssignableFrom(p))
				.FirstOrDefault(x => x.Name.Equals($"{subjectType}Creator"));

			Creator creator = null;
			if (creatorType != null)
			{
				creator = (Creator)Activator.CreateInstance(creatorType, new object[] { authenticator, message.Args });
			}			

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