using System.Net.Http;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class ExpensesApi
	{
		public Expense Create(HttpAuthenticator authenticator, Expense expense)
		{
			return ApiHelper.GetItem<Expense>(authenticator, "Expenses/Create", HttpMethod.Put, expense);
		}
	}
}
