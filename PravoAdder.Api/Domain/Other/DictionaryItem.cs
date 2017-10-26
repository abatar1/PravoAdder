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
	}
}