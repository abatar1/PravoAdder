using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Domain;
using PravoAdder.Domain.Info;

namespace PravoAdder.Controllers
{
    public class MigrationProcessController : DatabaseFiller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static int _count;
        private readonly Settings _settings;

        public MigrationProcessController(HttpAuthenticator httpAuthenticator, Settings settings) :
            base(httpAuthenticator)
        {
            _settings = settings;
        }

        public string AddProjectGroup(HeaderBlockInfo headerBlock)
        {
            var projectGroupSender = AddProjectGroupAsync(_settings, headerBlock).Result;
            if (projectGroupSender.Type == EnviromentMessageType.Error) Logger.Error($"{projectGroupSender.Message}");
            return projectGroupSender.Content;
        }

        public string AddProject(HeaderBlockInfo headerBlock, string projectGroupId)
        {
            var projectSender = AddProjectAsync(_settings, headerBlock, projectGroupId).Result;
            if (projectSender.Type == EnviromentMessageType.Error) Logger.Error($"{projectSender.Message}");
            return projectSender.Content;
        }

        public void AddInformationAsync(BlockInfo blockInfo, IDictionary<int, string> excelRow,
            string projectId)
        {
            var informationSender = AddInformationAsync(projectId, blockInfo, excelRow).Result;
            if (informationSender.Type == EnviromentMessageType.Error) Logger.Error($"{informationSender.Message}");        
        }

	    public void Synchronize(string projectId, string syncNum)
	    {
		    var syncSender = SynchronizeWithKad(projectId, syncNum).Result;
			if (syncSender.Type == EnviromentMessageType.Error) Logger.Error($"{syncSender.Message}");
		}

		public void ProcessCount(int current, int total, HeaderBlockInfo headerInfo, string projectId)
		{
			var projectName = headerInfo.ProjectName;
			if (projectName.Length > 50)
			{
				var lastSpacePosition = projectName.LastIndexOf(" ", 0, 50, StringComparison.Ordinal);
				projectName = $"{projectName.Remove(0, lastSpacePosition)}...";
			}

			_count += 1;
            Logger.Info(
                $"{DateTime.Now} | Progress: {current}/{total} ({_count}) | Name: {projectName} | Id: {projectId}");
        }
    }
}