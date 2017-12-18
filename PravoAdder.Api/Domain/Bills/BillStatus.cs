using System;
using System.Collections.Generic;
using System.Linq;

namespace PravoAdder.Api.Domain
{
	public class BillStatus : DatabaseEntityItem
	{
		private static List<BillStatus> _billStatuses;

		public static BillStatus GetStatus(HttpAuthenticator authenticator, string name)
		{
			if (_billStatuses == null) _billStatuses = ApiRouter.Bootstrap.GetBillsStatus(authenticator);
			return string.IsNullOrEmpty(name)
				? null
				: _billStatuses.FirstOrDefault(b => b.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
		}
	}
}
