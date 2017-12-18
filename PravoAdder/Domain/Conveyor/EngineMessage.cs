using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Readers;
using PravoAdder.Wrappers;

namespace PravoAdder.Domain
{
	public class EngineMessage : IDisposable
	{			
		// Settings
		public int Count { get; set; }
		public int Total { get; set; }
		public bool IsUpdate { get; set; }
		public bool IsFinal { get; set; }
		public bool IsContinue { get; set; }
		public ParallelOptions ParallelOptions { get; set; }
		public Settings Settings { get; set; }	
		
		// Wrappers
		public Counter Counter { get; set; }	
		public HttpAuthenticator Authenticator { get; set; }
		public ApiEnviroment ApiEnviroment { get; set; }
		public CaseBuilder CaseBuilder { get; set; }
		public IDictionary<string, Creator> Creators { get; set; }

		// Data
		public List<ConveyorItem> Child { get; set; }
		public Table Table { get; set; }
		public HeaderBlockInfo HeaderBlock { get; set; }
		public Row Row { get; set; }
		public DatabaseEntityItem Item { get; set; }
		public DatabaseEntityItem ConstructedItem { get; set; }

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

		public TType GetCreatable<TType>(DatabaseEntityItem item = null) where TType : ICreatable
		{
			return (TType) Creators[typeof(TType).Name + "Creator"].Create(Table.Header, Row, item);
		}

		public string GetValueFromRow(string name)
		{
			return Table.GetValue(Table.Header, Row, name);
		}

		#region IDisposable Support

		private bool _disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					Authenticator?.Dispose();
				}

				_disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}