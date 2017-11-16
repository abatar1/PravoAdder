using System;
using System.Collections.Generic;
using PravoAdder.Api;
using PravoAdder.Api.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Repositories;
using PravoAdder.Domain;
using PravoAdder.Helpers;

namespace PravoAdder.Readers
{
	public class EventCreator : ICreator
	{
		public EventCreator(HttpAuthenticator httpAuthenticator)
		{
			HttpAuthenticator = httpAuthenticator;
		}

		public HttpAuthenticator HttpAuthenticator { get; }

		public ICreatable Create(Row header, Row row, DatabaseEntityItem item = null)
		{
			var eventTypeName = Table.GetValue(header, row, "Activity Type");
			var eventType = EventTypeRepository.GetOrPut(HttpAuthenticator, eventTypeName);
			if (eventType == null) return null;

			var projectName = Table.GetValue(header, row, "Case Name");
			var project = ProjectRepository.GetDetailed<ProjectsApi>(HttpAuthenticator, projectName);			
			if (project == null) return null;			

			var calendar = CalendarRepository.Get<CalendarApi>(HttpAuthenticator, "Firm");

			var endDate = Table.GetValue(header, row, "Log Date").FormatDate();
			var logTimer = int.Parse(Table.GetValue(header, row, "Timer"));
			var startDate = endDate.Subtract(new TimeSpan(0, logTimer, 0));
			return new Event
			{
				Name = Table.GetValue(header, row, "Event Name"),		
				EventType = eventType,
				Description = Table.GetValue(header, row, "Description"),
				StartDate = startDate,
				EndDate = endDate,
				Project = project,
				AllDay = false,
				Calendar = calendar,
				TimeLogs = new List<string> {item?.Id}
			};
		}
	}
}
