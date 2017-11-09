﻿using System;

namespace PravoAdder.Api.Domain
{
	public abstract class DatabaseEntityItem : IEquatable<DatabaseEntityItem>
	{
		public string Name { get; set; }
		public string Id { get; set; }
		public string SysName { get; set; }
		public string DisplayName { get; set; }

		public override string ToString()
		{
			return Name ?? DisplayName;
		}

		public bool Equals(DatabaseEntityItem other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(Id, other.Id);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((DatabaseEntityItem) obj);
		}

		public override int GetHashCode()
		{
			return (Id != null ? Id.GetHashCode() : 0);
		}
	}
}
