using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;
using PravoAdder.Domain;

namespace PravoAdder.Wrappers
{
    public class ApiEnviroment
    {
	    private const int MaxWordLength = 350;
		private readonly HttpAuthenticator _httpAuthenticator;
	    private readonly FieldBuilder _fieldBuilder;
	    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		public ApiEnviroment(HttpAuthenticator httpAuthenticator)
        {
            _httpAuthenticator = httpAuthenticator;
	        _fieldBuilder = new FieldBuilder(httpAuthenticator);
        }      

	    public async void SynchronizeCase(string projectId, string syncNumber)
	    {
		    var isSuccessResponse = await ApiRouter.Casebook.CheckCasebookCaseAsync(_httpAuthenticator, projectId, syncNumber);
			if (!isSuccessResponse) Logger.Error($"Failed to synchronize case {projectId}");
		}

	    public void DeleteFolderItem(string folderId)
	    {
		    try
		    {
				ApiRouter.ProjectFolders.Delete(_httpAuthenticator, folderId);
		    }
			catch (Exception e)
		    {
				Logger.Error($"Folder deleting failed. Reason: {e.Message}");
		    }
		}

	    public void DeleteProjectGroupItem(string projectGroupId)
	    {
		    try
		    {
			    ApiRouter.ProjectGroups.ArchiveProjectGroup(_httpAuthenticator, projectGroupId);
				ApiRouter.ProjectGroups.DeleteProjectGroup(_httpAuthenticator, projectGroupId);			
		    }
		    catch (Exception e)
		    {
			    Logger.Error($"Project group deleting failed. Reason: {e.Message}");
		    }
		}

	    public void DeleteProjectItem(string projectId)
	    {
		    try
		    {
			    ApiRouter.Projects.ArchiveProject(_httpAuthenticator, projectId);
			    ApiRouter.Projects.DeleteProject(_httpAuthenticator, projectId);
			}
		    catch (Exception e)
		    {
			    Logger.Error($"Project deleting failed. Reason: {e.Message}");
		    }
		}

	    public IList<ProjectGroup> GetProjectGroupItems()
	    {
		    var response = ApiRouter.ProjectGroups.GetProjectGroups(_httpAuthenticator);
		    if (response == null) Logger.Error("No project groups found");
		    return response;
	    }

	    public IList<ProjectFolder> GetProjectFolderItems()
	    {
		    var response = ApiRouter.ProjectFolders.GetProjectFolders(_httpAuthenticator);
		    if (response == null) Logger.Error("No project folders found");
		    return response;
	    }

	    public GroupedProjects GetGroupedProjects(string projectGroupId, string folderName = null)
	    {
		    var response = ApiRouter.Projects.GetGroupedProjects(_httpAuthenticator, folderName, projectGroupId);
		    if (response == null) Logger.Error($"No projects found at group {projectGroupId}");
			return response;
	    }

		public ProjectGroup AddProjectGroup(Settings settings, HeaderBlockInfo headerInfo)
        {
	        if (headerInfo.ProjectGroupName == null) return null;		

			if (settings.Overwrite)
			{
				var response = GetProjectGroupItems();
				var projectGroupResponse = response?.GetByName(headerInfo.ProjectGroupName);
				if (projectGroupResponse != null) return projectGroupResponse;
			}

	        var projectFolder = ApiRouter.ProjectFolders.GetProjectFolders(_httpAuthenticator)
		        .GetByName(headerInfo.FolderName);
	        if (projectFolder == null)
	        {
		        ApiRouter.ProjectFolders.InsertProjectFolder(headerInfo.FolderName, _httpAuthenticator);
			}               

            var content = new
            {
                Name = headerInfo.ProjectGroupName,
                ProjectFolder = projectFolder,
                headerInfo.Description
            };

	        var projectGroup = ApiRouter.ProjectGroups.ProjectGroups(_httpAuthenticator, content);
			if (projectGroup == null) Logger.Error($"Failed to add {headerInfo.ProjectGroupName} project group");
	        return projectGroup;			
		}

        public Project AddProject(Settings settings, HeaderBlockInfo headerInfo, string projectGroupId)
        {
	        if (settings.Overwrite)
	        {
		        var response = GetGroupedProjects(projectGroupId, headerInfo.FolderName);
		        if (response != null)
		        {
					var projects = response.Projects;
			        var projectResponse = projects.GetByName(headerInfo.ProjectName);
			        if (projectResponse != null) return projectResponse;
				}
            }

	        if (string.IsNullOrEmpty(headerInfo.ProjectName)) headerInfo.ProjectName = "Название проекта по-умолчанию";

			var projectFolder = ApiRouter.ProjectFolders.GetProjectFolders(_httpAuthenticator)
		        .GetByName(headerInfo.FolderName);
	        if (projectFolder == null)
	        {
		        ApiRouter.ProjectFolders.InsertProjectFolder(headerInfo.FolderName, _httpAuthenticator);
			}

	        var projectType = ApiRouter.ProjectTypes.GetProjectTypes(_httpAuthenticator)
		        .GetByName(headerInfo.ProjectTypeName);
	        if (projectType == null)
	        {
		        Logger.Error($"Project type {headerInfo.ProjectTypeName} doesn't exist. Project name: {headerInfo.ProjectName}");
		        return null;
	        }

	        var responsible = ApiRouter.Responsibles.GetResponsibles(_httpAuthenticator)
		        .GetByName(headerInfo.ResponsibleName.Replace(".", ""));
	        if (responsible == null)
	        {
				Logger.Error($"Responsible {headerInfo.ResponsibleName} doesn't exist. Project name: {headerInfo.ProjectName}");
		        return null;
	        }

	        var projectGroup = headerInfo.ProjectGroupName == null && projectGroupId == null
		        ? null
		        : new ProjectGroup(headerInfo.ProjectGroupName, projectGroupId);

			var projectName = headerInfo.ProjectName;
	        if (projectName.Length > MaxWordLength) projectName = projectName.Remove(MaxWordLength);

			var content = new
            {
                ProjectFolder = projectFolder,
                ProjectType = projectType,
                Responsible = responsible,
                ProjectGroup = projectGroup,
                Name = projectName
			};

	        var project = ApiRouter.Projects.CreateProject(_httpAuthenticator, content);
			if (project == null) Logger.Error($"Failed to add {headerInfo.ProjectName} project");
			return project;
		}

        public async void AddInformationAsync(BlockInfo blockInfo,
            IDictionary<int, string> excelRow, string projectId, int order)
        {		
			var contentLines = new List<BlockLineInfo>();
	        if (blockInfo.Lines == null || !blockInfo.Lines.Any()) return;

            var messageBuilder = new StringBuilder();	        
			foreach (var line in blockInfo.Lines)
            {
                var contentFields = new List<BlockFieldInfo>();
                foreach (var fieldInfo in line.Fields)
                {
                    if (!excelRow.ContainsKey(fieldInfo.ColumnNumber))
                    {
                        messageBuilder.AppendLine($"Excel row doesn't contain \"{fieldInfo.ColumnNumber}\" key.");
                        continue;
                    }
                    var fieldData = excelRow[fieldInfo.ColumnNumber];

                    if (string.IsNullOrEmpty(fieldData)) continue;

                    try
                    {
                        var value = _fieldBuilder.CreateFieldValueFromData(fieldInfo, fieldData);
                        var newFieldInfo = fieldInfo.CloneWithValue(value);
                        contentFields.Add(newFieldInfo);
                    }
                    catch (Exception e)
                    {
	                    messageBuilder.AppendLine(
		                    $"Error while reading value from table! Message: {e.Message} Id: {projectId} Data: {fieldData} Type: {fieldInfo.Type}");
                    }
                }
                if (!contentFields.Any()) continue;
                var newLine = line.CloneWithFields(contentFields);
                contentLines.Add(newLine);
            }

            if (contentLines.All(c => !c.Fields.Any())) return;

            var contentBlock = new
            {
                VisualBlockId = blockInfo.Id,
                ProjectId = projectId,
                Lines = contentLines,
                FrontOrder = order,
				Order = order
			};

	        var isSuccessResponse = await ApiRouter.ProjectCustomValues.Create(_httpAuthenticator, contentBlock);
	        if (!isSuccessResponse)
	        {
		        Logger.Error(
			        $"{DateTime.Now} | Failed to add information block {blockInfo.Name}. {messageBuilder.ToString().Trim()} | Id : {projectId}");
	        }
        }
    }
}