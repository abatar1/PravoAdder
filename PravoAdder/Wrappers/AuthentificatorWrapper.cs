using System;
using System.Security.Authentication;
using NLog;
using PravoAdder.Api;
using PravoAdder.Domain;

namespace PravoAdder.Wrappers
{
    public class AuthentificatorWrapper : HttpAuthenticator
	{
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
		private readonly ApplicationArguments _arguments;

        public AuthentificatorWrapper(ApplicationArguments args) : base(args.BaseUri)
        {
	        _arguments = args;
        }

        public HttpAuthenticator Authenticate()
        {
            Logger.Info($"Login as {_arguments.UserName} to {_arguments.BaseUri}...");
	        try
	        {
		        Authentication(_arguments.UserName, _arguments.Password);
		        return this;
	        }
	        catch (AuthenticationException e)
	        {
		        Logger.Error($"Failed to login in. Message: {e.Message}");
	        }
	        catch (Exception e)
	        {
				Logger.Error($"Unknown exception while logging in. Message: {e.Message}");
			}
	        return null;
        }
    }
}