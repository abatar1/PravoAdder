namespace PravoAdder.Api.Domain
{
    public class DictionaryItem : DatabaseEntityItem
	{
        public DictionaryItem(string name, string id) : base(name, id)
        {
        }

	    public DictionaryItem(object data) : base(data)
	    {
		    LetterCode = ((dynamic) data)?.LetterCode;
	    }

		public DictionaryItem()
		{
		}
		public string SystemName { get; set; }
		public string LetterCode { get; }
	}
}