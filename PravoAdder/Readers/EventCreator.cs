using System;
using System.Collections.Generic;
using System.Globalization;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Repositories;
using PravoAdder.Domain;

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

			var projectName = Table.GetValue(header, row, "Case Name");
			var project = ProjectRepository.GetDetailed(HttpAuthenticator, projectName);			
			if (project == null) return null;			

			var calendar = CalendarRepository.Get(HttpAuthenticator, "Firm");

			var rawEndDate = Table.GetValue(header, row, "End Date").Replace("UTC", "").Trim();
			DateTime.TryParseExact(rawEndDate, new[] {"MM/dd/yyyy hh:mm tt", "MM/dd/yyyy h:mm tt"}, CultureInfo.InvariantCulture,
				DateTimeStyles.None, out var date);
			var endDate = date.ToString("o");
			var startDate = DateTime.Parse(Table.GetValue(header, row, "Log Date")).ToString("o");
			return new Event
			{
				Name = Table.GetValue(header, row, "Name"),		
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
