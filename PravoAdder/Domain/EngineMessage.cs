using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Readers;
using PravoAdder.Wrappers;

namespace PravoAdder.Domain
{
	[Serializable]
	public class EngineMessage : IDisposable
	{
		public ApplicationArguments ApplicationArguments { get; set; }
		public Settings Settings { get; set; }
		public Table Table { get; set; }		
		public Row Row { get; set; }
		public int Count { get; set; }
		public int Total { get; set; }
		public bool IsUpdate { get; set; }
		public DatabaseEntityItem Item { get; set; }
		public HeaderBlockInfo HeaderBlock { get; set; }
		public Counter Counter { get; set; }
		public ParallelOptions ParallelOptions { get; set; }	
		public string DateTime { get; set; }

		public HttpAuthenticator Authenticator { get; set; }
		public ApiEnviroment ApiEnviroment { get; set; }
		public CaseCreator CaseCreator { get; set; }
		public TaskCreator TaskCreator { get; set; }
		public ParticipantCreator ParticipantCreator { get; set; }

		public List<ConveyorItem> Child { get; set; }

		public void Concat(EngineMessage other)
		{
			foreach (var property in typeof(EngineMessage).GetProperties())
			{
				var propertyType = property.PropertyType;
				var defaultValue = propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null;				
				if (property.GetValue(this) == defaultValue)
				{
					property.SetValue(this, property.GetValue(other));
				}
			}
		}

		public void Dispose()
		{
			Authenticator.Dispose();
		}
	}
}