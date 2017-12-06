using System;
using NLog;
using PravoAdder.Api.Domain;
using PravoAdder.Helpers;

namespace PravoAdder.Wrappers
{
	public class Counter
	{
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
		private int _count;

		public Counter()
		{
			_count = 0;
		}

		public void ProcessCount(int current, int total, int rowOffset, DatabaseEntityItem item, int sliceNum = int.MaxValue)
		{
			_count += 1;

			var itemName = item.Name ?? item.DisplayName;

			Logger.Info(
				$"{DateTime.Now} | Progress: {current + 1 + rowOffset}/{total + rowOffset} ({_count}) | Name: {itemName.SliceSpaceIfMore(sliceNum)} | Id: {item.Id}");
		}

		public void Message(string message)
		{
			_count += 1;

			Logger.Info(
				$"{DateTime.Now} | Message: {message}");
		}
	}
}
