using System;
using System.Collections.Generic;
using NLog;
using PravoAdder.DatabaseEnviroment;
using PravoAdder.Domain;
using PravoAdder.Domain.Info;
using PravoAdder.Api;
using PravoAdder.Api.Domain;

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

        public DatabaseEntityItem AddProjectGroup(HeaderBlockInfo headerBlock)
        {
            var projectGroupSender = AddProjectGroup(_settings, headerBlock);
            if (projectGroupSender.Type == EnviromentMessageType.Error) Logger.Error($"{projectGroupSender.Message}");
            return projectGroupSender.Content;
        }

        public DatabaseEntityItem AddProject(HeaderBlockInfo headerBlock, string projectGroupId)
        {
            var projectSender = AddProject(_settings, headerBlock, projectGroupId);
            if (projectSender.Type == EnviromentMessageType.Error) Logger.Error($"{projectSender.Message}");
            return projectSender.Content;
        }

        public void AddInformationAsync(BlockInfo blockInfo, IDictionary<int, string> tableRow,
            string projectId, int order)
        {
            var informationSender = AddInformationAsync(projectId, blockInfo, tableRow, order).Result;
            if (informationSender.Type == EnviromentMessageType.Error) Logger.Error($"{informationSender.Message}");        
        }

	    public void Synchronize(string projectId, string syncNum)
	    {
		    var syncSender = SynchronizeCase(projectId, syncNum).Result;
			if (syncSender.Type == EnviromentMessageType.Error) Logger.Error($"{syncSender.Message}");
		}

		public void ProcessCount(int current, int total, DatabaseEntityItem project, int sliceNum = int.MaxValue)
		{
			var projectName = project.Name;
			if (projectName.Length > sliceNum)
			{
				var lastSpacePosition = projectName.LastIndexOf(' ', sliceNum);
				projectName = $"{projectName.Remove(lastSpacePosition)}...";
			}

			_count += 1;
            Logger.Info(
                $"{DateTime.Now} | Progress: {current}/{total} ({_count}) | Name: {projectName} | Id: {project.Id}");
        }
    }
}