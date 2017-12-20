using System;
using System.Collections.Generic;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Repositories;
using PravoAdder.Domain;
using PravoAdder.Helpers;

namespace PravoAdder.Readers
{
	public class EventCreator : Creator
	{
		public override ICreatable Create(Table table, Row row, DatabaseEntityItem item = null)
		{
			var projectName = table.GetValue(row, "Case Name");
			var project = ProjectRepository.GetDetailed(HttpAuthenticator, projectName);			
			if (project == null) return null;			

			var calendar = CalendarRepository.Get(HttpAuthenticator, "Firm");

			var newEvent = new Event
			{
				Name = table.GetValue(row, "Event Name").SliceSpaceIfMore(100),
				Description = table.GetValue(row, "Event Description").SliceSpaceIfMore(100),
				Project = project,
				AllDay = false,
				Calendar = calendar,
				TimeLogs = new List<string> { item?.Id }
			};

			if (table.TryGetValue(row, "Event Type", out var value))
			{
				newEvent.EventType = EventTypeRepository.GetOrPut(HttpAuthenticator, value);
			}

			var endDateValue = table.GetValue(row, "Log Date");
			if (endDateValue == null)
			{
				newEvent.EndDate = DateTime.Today;
				newEvent.StartDate = DateTime.Today;
			}
			else
			{
				newEvent.EndDate = DateTime.Parse(endDateValue);
				var logTimer = int.Parse(table.GetValue(row, "Timer"));
				newEvent.StartDate = newEvent.EndDate.Subtract(new TimeSpan(0, logTimer, 0));
			}

			return newEvent;
		}

		public EventCreator(HttpAuthenticator httpAuthenticator, Settings settings) : base(httpAuthenticator, settings)
		{
		}
	}
}
