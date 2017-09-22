using System;
using System.Diagnostics.Contracts;
using NLog;
using PravoAdder.Api;
using PravoAdder.Domain;

namespace PravoAdder.Controllers
{
    public class AuthentificatorController : HttpAuthenticator
	{
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Settings _settings;

        public AuthentificatorController(Settings settings) : base(settings.BaseUri)
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
		            var message = Authentication(_settings.Login, _settings.Password);
		            if (message.Type != EnviromentMessageType.Error) return this;
		            Logger.Error($"Failed to login in. Message: {message.Message}");
				}
	            catch (Exception e)
	            {
		            Logger.Error($"Failed to login in. Message: {e.Message}");
		            return null;		            
	            }                             				
            }
        }
    }
}