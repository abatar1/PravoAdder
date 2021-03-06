﻿using System;
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

        public AuthentificatorWrapper(Settings settings, TimeSpan timeSpan, int maxRetries) : base(settings.BaseUri, timeSpan, maxRetries)
        {
	        _settings = settings;
        }

        public HttpAuthenticator Authenticate()
        {
            Logger.Info($"Login as {_settings.UserName} to {_settings.BaseUri}...");
	        try
	        {
		        Authentication(_settings.UserName, _settings.Password);
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