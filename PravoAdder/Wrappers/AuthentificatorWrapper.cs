﻿using System;
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
		            var message = Authentication(_settings.Login, _settings.Password);
		            if (message.MessageType != EnviromentMessageType.Error) return this;
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