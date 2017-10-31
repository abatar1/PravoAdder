namespace PravoAdder.Helpers
{
	public static class StringHelper
	{
		public static string SliceSpaceIfMore(this string item, int threshold)
		{
			if (item == null || item.Length <= threshold) return item;

			var lastSpacePosition = item.LastIndexOf(' ', threshold);
			return $"{item.Remove(lastSpacePosition)}";
		}
	}
}
