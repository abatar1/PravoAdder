namespace PravoAdder.Api.Domain
{
    public class DictionaryItem : DatabaseEntityItem
	{
		public DictionaryItem()
		{
		}

		public DictionaryItem(string name, string id)
		{
			Name = name;
			Id = id;
		}

		public string SystemName { get; set; }
		public string LetterCode { get; set; }

		public static explicit operator EventType(DictionaryItem other)
		{
			return new EventType {Name = other.Name, Id = other.Id, SysName = other.SysName};
		}
	}
}