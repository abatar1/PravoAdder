using System;
using System.Collections.Generic;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Domain;

namespace PravoAdder.Readers
{
	public class BillCreator : Creator
	{
		public override ICreatable Create(Row header, Row row, DatabaseEntityItem item = null)
		{
			if (item == null) return null;

			var hasUnbilled = ApiRouter.Bills.HasCaseUnbilledTimes(HttpAuthenticator, item.Id);
			if (!hasUnbilled) return null;

			var bill = ApiRouter.Bills.Create(HttpAuthenticator, item.Id);
			string statusName;
			try
			{
				statusName = Table.GetValue(header, row, "Bill Status");
			}
			catch (Exception)
			{
				statusName = "Draft";
			}
			var billStatus = BillStatus.GetStatus(HttpAuthenticator, statusName);
			if (billStatus == null) return null;

			ApiRouter.Bills.UpdateStatus(HttpAuthenticator, new BillStatusGroup { BillIds = new List<string> { bill.Id }, BillStatusSysName = billStatus.SysName });
			return bill;
		}

		public BillCreator(HttpAuthenticator httpAuthenticator, Settings settings) : base(httpAuthenticator, settings)
		{
		}
	}
}
