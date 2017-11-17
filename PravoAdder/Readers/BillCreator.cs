using System.Collections.Generic;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Repositories;
using PravoAdder.Domain;

namespace PravoAdder.Readers
{
	public class BillCreator : Creator
	{
		public override ICreatable Create(Row header, Row row, DatabaseEntityItem item = null)
		{
			var project = ProjectRepository.GetDetailed<ProjectsApi>(HttpAuthenticator, Table.GetValue(header, row, "Case Name"));
			if (project == null) return null;

			if (project.Client == null)
			{
				var clientName = Table.GetValue(header, row, "Client");
				if (string.IsNullOrEmpty(clientName)) return null;
				var names = clientName.Split(' ');
				if (names.Length != 2) return null;

				var client = new Participant(names[0], names[1],
					ParticipantType.GetPersonType(HttpAuthenticator));
				project.Client = ParticipantsRepository.GetOrCreate<ParticipantsApi>(HttpAuthenticator, clientName, client);
				project = ApiRouter.Projects.Put(HttpAuthenticator, project);
			}

			var hasUnbilled = ApiRouter.Bills.HasCaseUnbilledTimes(HttpAuthenticator, project.Id);
			if (!hasUnbilled) return null;

			var bill = ApiRouter.Bills.Create(HttpAuthenticator, project.Id);
			var billStatus = BillStatus.GetStatus(HttpAuthenticator, Table.GetValue(header, row, "Status"));
			ApiRouter.Bills.UpdateStatus(HttpAuthenticator, new BillStatusGroup { BillIds = new List<string> { bill.Id }, BillStatusSysName = billStatus.SysName });
			return bill;
		}

		public BillCreator(HttpAuthenticator httpAuthenticator, ApplicationArguments applicationArguments) : base(httpAuthenticator, applicationArguments)
		{
		}
	}
}
