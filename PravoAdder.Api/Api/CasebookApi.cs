using System.Net.Http;
using System.Threading.Tasks;
using PravoAdder.Api.Helpers;

namespace PravoAdder.Api
{
	public class CasebookApi
	{
		public async Task<bool> CheckCasebookCaseAsync(HttpAuthenticator httpAuthenticator, string id, string syncNumber)
		{
			var content = new
			{
				ProjectId = id,
				CasebookNumber = syncNumber
			};

			return await ApiHelper.TrySendAsync(httpAuthenticator, "Casebook/CheckCasebookCase", HttpMethod.Put, content);
		}
	}
}
