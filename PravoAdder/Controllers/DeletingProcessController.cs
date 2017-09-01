using PravoAdder.DatabaseEnviroment;

namespace PravoAdder.Controllers
{
	public class DeletingProcessController : DatabaseCleaner
	{
		public DeletingProcessController(HttpAuthenticator authenticator) : base(authenticator)
		{
		}
	}
}
