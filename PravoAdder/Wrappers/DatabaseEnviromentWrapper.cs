using System;
using System.Collections.Generic;
using NLog;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Domain;

namespace PravoAdder.Wrappers
{
    public class DatabaseEnviromentWrapper : DatabaseEnviroment
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static int _count;
        private readonly Settings _settings;

        public DatabaseEnviromentWrapper(HttpAuthenticator httpAuthenticator, Settings settings) :
            base(httpAuthenticator)
        {
            _settings = settings;
        }

	    public void DeleteFolder(string folderId)
	    {
			var sender = DeleteFolderItem(folderId);
		    if (sender.MessageType == EnviromentMessageType.Error) Logger.Error($"{sender.Message}");
		}

	    public void DeleteProjectGroup(string projectGroupId)
	    {
		    var sender = DeleteProjectGroupItem(projectGroupId);
		    if (sender.MessageType == EnviromentMessageType.Error) Logger.Error($"{sender.Message}");
		}

	    public void DeleteProject(string projectId)
	    {
		    var sender = DeleteProjectItem(projectId);
		    if (sender.MessageType == EnviromentMessageType.Error) Logger.Error($"{sender.Message}");
		}

	    public IList<DatabaseEntityItem> GetProjectGroups()
	    {
		    var sender = GetProjectGroupItems();
			if (sender.MessageType == EnviromentMessageType.Error) Logger.Error($"{sender.Message}");
		    return sender.MultipleContent;
	    }

	    public IList<DatabaseEntityItem> GetProjectFolders()
	    {
		    var sender = GetProjectFolderItems();
		    if (sender.MessageType == EnviromentMessageType.Error) Logger.Error($"{sender.Message}");
		    return sender.MultipleContent;
		}

	    public DatabaseEntityItem GetGroupedProjects(string projectGroupId, string folderName = null)
	    {
		    var sender = GetProjectItems(projectGroupId, folderName);
		    if (sender.MessageType == EnviromentMessageType.Error) Logger.Error($"{sender.Message}");
		    return sender.SingleContent;
		}

	    public DatabaseEntityItem AddProject(HeaderBlockInfo headerBlock, string projectGroupId)
	    {
			var projectSender = AddProject(_settings, headerBlock, projectGroupId);
		    if (projectSender.MessageType == EnviromentMessageType.Error) Logger.Error($"{projectSender.Message}");
		    return projectSender.SingleContent;
		}

	    public DatabaseEntityItem AddProjectGroup(HeaderBlockInfo headerBlock)
	    {
		    var projectSender = AddProjectGroup(_settings, headerBlock);
		    if (projectSender.MessageType == EnviromentMessageType.Error) Logger.Error($"{projectSender.Message}");
		    return projectSender.SingleContent;
	    }

		public void AddInformationAsync(BlockInfo blockInfo, IDictionary<int, string> tableRow,
            string projectId, int order)
        {
            var informationSender = AddInformationAsync(projectId, blockInfo, tableRow, order).Result;
            if (informationSender.MessageType == EnviromentMessageType.Error) Logger.Error($"{informationSender.Message}");        
        }

	    public void Synchronize(string projectId, string syncNum)
	    {
		    var syncSender = SynchronizeCase(projectId, syncNum).Result;
			if (syncSender.MessageType == EnviromentMessageType.Error) Logger.Error($"{syncSender.Message}");
		}

		public void ProcessCount(int current, int total, DatabaseEntityItem item, int sliceNum = int.MaxValue)
		{
			var itemName = item.Name;
			if (itemName != null && itemName.Length > sliceNum)
			{
				var lastSpacePosition = itemName.LastIndexOf(' ', sliceNum);
				itemName = $"{itemName.Remove(lastSpacePosition)}...";
			}

			_count += 1;
            Logger.Info(
                $"{DateTime.Now} | Progress: {current}/{total} ({_count}) | Name: {itemName} | Id: {item.Id}");
        }
    }
}