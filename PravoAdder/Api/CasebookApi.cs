using System.Net.Http;
using System.Threading.Tasks;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Helpers;

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

			return await ApiHelper.TrySendAsync(httpAuthenticator, content, "Casebook/CheckCasebookCase", HttpMethod.Put);
		}
	}
}
