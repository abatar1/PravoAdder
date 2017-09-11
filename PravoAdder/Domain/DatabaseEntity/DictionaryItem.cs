namespace PravoAdder.Domain
{
    public class DictionaryItem
    {
        public DictionaryItem(string name, string id)
        {
            Name = name;
            Id = id;
        }

        public string Name { get; }
        public string Id { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}