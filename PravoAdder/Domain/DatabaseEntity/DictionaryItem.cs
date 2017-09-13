namespace PravoAdder.Domain
{
    public class DictionaryItem : DatabaseEntityItem
	{
        public DictionaryItem(string name, string id) : base(name, id)
        {
        }

	    public DictionaryItem(object data) : base(data)
	    {
	    }

		public DictionaryItem()
		{
		}
	}
}