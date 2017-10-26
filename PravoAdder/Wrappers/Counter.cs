﻿using System;
using NLog;
using PravoAdder.Api.Domain;

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

		public void ProcessCount(int current, int total, DatabaseEntityItem item, int sliceNum = int.MaxValue)
		{
			_count += 1;

			var itemName = item.Name ?? item.DisplayName;
			
			if (itemName != null && itemName.Length > sliceNum)
			{
				var lastSpacePosition = itemName.LastIndexOf(' ', sliceNum);
				itemName = $"{itemName.Remove(lastSpacePosition)}...";
			}
			
			Logger.Info($"{DateTime.Now} | Progress: {current + 1}/{total} ({_count}) | Name: {itemName} | Id: {item?.Id}");
		}
	}
}
