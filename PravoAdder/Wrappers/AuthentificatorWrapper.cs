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
        private readonly Settings _settings;

        public AuthentificatorWrapper(Settings settings) : base(settings.BaseUri)
        {
            _settings = settings;
        }

        public HttpAuthenticator Authenticate()
        {
            while (true)
            {
                Logger.Info($"Login as {_settings.Login} to {_settings.BaseUri}...");
	            try
	            {
		            Authentication(_settings.Login, _settings.Password);
		            return this;
	            }
	            catch (AuthenticationException e)
	            {
		            Logger.Error($"Failed to login in. Message: {e.Message}");
		            throw;
	            }
	            catch (Exception e)
	            {
					Logger.Error($"Unknown exception while logging in. Message: {e.Message}");
		            throw;
				}
            }
        }
    }
}