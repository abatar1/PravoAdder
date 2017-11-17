using System.Collections.Generic;
using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ExpensesApi : IApi<Expense>
	{
		public List<Expense> GetMany(HttpAuthenticator authenticator, string projectId)
		{
			var parameter = ApiHelper.CreateParameters(("projectId", projectId));
			return ApiHelper.GetItems<Expense>(authenticator, "Expenses/GetExpenses", HttpMethod.Post, parameter);
		}

		public Expense Get(HttpAuthenticator authenticator, string parameter)
		{
			throw new System.NotImplementedException();
		}

		public Expense Create(HttpAuthenticator authenticator, Expense expense)
		{
			return ApiHelper.GetItem<Expense>(authenticator, "Expenses/Create", HttpMethod.Put, expense);
		}
	}
}
