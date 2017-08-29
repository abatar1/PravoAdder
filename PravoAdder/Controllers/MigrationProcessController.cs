using System;
using System.Collections.Generic;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Domain;
using PravoAdder.Domain.Info;
using NLog;

namespace PravoAdder.Controllers
{
    public class MigrationProcessController
    {
	    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public HttpAuthenticator Authenticate(Settings settings)
        {
            while (true)
            {              
                Logger.Info($"Login as {settings.Login} to {settings.BaseUri}...");
                var authentication = new HttpAuthenticator(settings.BaseUri);
                var message = authentication.Authentication(settings.Login, settings.Password);

	            if (message.Type != EnviromentMessageType.Error) return authentication;
	            Logger.Error($"Failed to login in. Message: {message.Message}");
            }
        }    	

        public string AddProjectGroup(HeaderBlockInfo headerBlock, DatabaseFiller filler, Settings settings)
        {
		    var projectGroupSender = filler.AddProjectGroupAsync(settings, headerBlock).Result;
			if (projectGroupSender.Type == EnviromentMessageType.Error) Logger.Error($"{projectGroupSender.Message}");
			return projectGroupSender.Content;			
		}

        public string AddProject(HeaderBlockInfo headerBlock, DatabaseFiller filler, Settings settings, string projectGroupId)
        {
		    var projectSender = filler.AddProjectAsync(settings, headerBlock, projectGroupId).Result;
			if (projectSender.Type == EnviromentMessageType.Error) Logger.Error($"{projectSender.Message}");
		    return projectSender.Content;
        }

        public void AddInformationAsync(BlockInfo blockInfo, DatabaseFiller filler, IDictionary<int, string> excelRow, string projectId)
        {
			var informationSender = filler.AddInformationAsync(projectId, blockInfo, excelRow).Result;
			if (informationSender.Type == EnviromentMessageType.Error) Logger.Error($"{informationSender.Message}");
		}

	    public void ProcessCount(int current, int total, HeaderBlockInfo headerInfo, string projectId)
	    {
		    Logger.Info($"{DateTime.Now} | Progress: {current}/{total} | Name: {headerInfo.ProjectName} | Id: {projectId}");
		}
    }
}
