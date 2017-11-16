using System;
using Newtonsoft.Json;

namespace PravoAdder.Api.Domain
{
	public abstract class DatabaseEntityItem : IEquatable<DatabaseEntityItem>
	{
		public string Name { get; set; }

		public string Id { get; set; }

		public string SysName { get; set; }

		public string DisplayName { get; set; }

		public virtual bool ShouldSerializeId() => true;

		[JsonIgnore]
		public bool WasDetailed { get; set; }

		public override string ToString()
		{
			return Name ?? DisplayName;
		}

		public bool Equals(DatabaseEntityItem other)
		{
			if (ReferenceEquals(null, other)) return false;
			return ReferenceEquals(this, other) || string.Equals(Id, other.Id);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj.GetType() == GetType() && Equals((DatabaseEntityItem) obj);
		}

		public override int GetHashCode() => Id != null ? Id.GetHashCode() : 0;		
	}
}
