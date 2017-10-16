using System;

namespace PravoAdder.Api.Domain
{
	public abstract class DatabaseEntityItem : IEquatable<DatabaseEntityItem>
	{
		public DatabaseEntityItem()
		{
			
		}

		public DatabaseEntityItem(string name, string id)
		{
			Name = name;
			Id = id;
		}

		public DatabaseEntityItem(object data)
		{
			var dynamicData = data as dynamic;
			Name = dynamicData?.Name?.ToString() ?? dynamicData?.DisplayName;
			Id = dynamicData?.Id?.ToString();
		}

		public string Name { get; set; }
		public string Id { get; set; }
		public string SysName { get; set; }

		public override string ToString()
		{
			return Name;
		}

		public bool Equals(DatabaseEntityItem other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(Name, other.Name) && string.Equals(Id, other.Id);
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
			unchecked
			{
				return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Id != null ? Id.GetHashCode() : 0);
			}
		}
	}
}
