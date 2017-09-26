using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PravoAdder.Domain;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder
{
    public class DatabaseEnviroment
    {
	    private const int MaxWordLength = 350;
		private readonly HttpAuthenticator _httpAuthenticator;
	    private readonly FieldBuilder _fieldBuilder;

        public DatabaseEnviroment(HttpAuthenticator httpAuthenticator)
        {
            _httpAuthenticator = httpAuthenticator;
	        _fieldBuilder = new FieldBuilder(httpAuthenticator);
        }      

	    protected async Task<EnviromentMessage> SynchronizeCase(string projectId, string syncNumber)
	    {
		    var isSuccessResponse = await ApiRouter.Casebook.CheckCasebookCaseAsync(_httpAuthenticator, projectId, syncNumber);
			return !isSuccessResponse
				? new EnviromentMessage(null, 
					$"Failed to synchronize case {projectId}",
					EnviromentMessageType.Error)
				: new EnviromentMessage(null, "Complete succefully.",
					EnviromentMessageType.Success);			
		}

	    protected EnviromentMessage DeleteFolderItem(string folderId)
	    {
		    try
		    {
				ApiRouter.ProjectFolders.Delete(_httpAuthenticator, folderId);
		    }
			catch (Exception e)
		    {
			    return new EnviromentMessage(null, $"Folder deleting failed. Reason: {e.Message}", EnviromentMessageType.Error);
		    }
		    return new EnviromentMessage(null, "Succefully deleted folder.", EnviromentMessageType.Success);
		}

	    protected EnviromentMessage DeleteProjectGroupItem(string projectGroupId)
	    {
		    try
		    {
			    ApiRouter.ProjectGroups.ArchiveProjectGroup(_httpAuthenticator, projectGroupId);
				ApiRouter.ProjectGroups.DeleteProjectGroup(_httpAuthenticator, projectGroupId);			
		    }
		    catch (Exception e)
		    {
			    return new EnviromentMessage(null, $"Project group deleting failed. Reason: {e.Message}", EnviromentMessageType.Error);
		    }
		    return new EnviromentMessage(null, "Succefully deleted project group.", EnviromentMessageType.Success);
		}

	    protected EnviromentMessage DeleteProjectItem(string projectId)
	    {
		    try
		    {
			    ApiRouter.Projects.ArchiveProject(_httpAuthenticator, projectId);
			    ApiRouter.Projects.DeleteProject(_httpAuthenticator, projectId);
			}
		    catch (Exception e)
		    {
			    return new EnviromentMessage(null, $"Project deleting failed. Reason: {e.Message}", EnviromentMessageType.Error);
		    }
			return new EnviromentMessage(null, "Succefully deleted project.", EnviromentMessageType.Success);
		}

	    protected EnviromentMessage GetProjectGroupItems()
	    {
		    var response = ApiRouter.ProjectGroups.GetProjectGroups(_httpAuthenticator);
		    return response == null 
				? new EnviromentMessage(null, "No project groups found", EnviromentMessageType.Error) 
				: new EnviromentMessage(response, "Succefully got project groups.", EnviromentMessageType.Success);
	    }

	    protected EnviromentMessage GetProjectFolderItems()
	    {
		    var response = ApiRouter.ProjectFolders.GetProjectFolders(_httpAuthenticator);
		    return response == null
			    ? new EnviromentMessage(null, "No project folders found", EnviromentMessageType.Error)
			    : new EnviromentMessage(response, "Succefully got project folders.", EnviromentMessageType.Success);
		}

	    protected EnviromentMessage GetProjectItems(string projectGroupId, string folderName = null)
	    {
		    var response = ApiRouter.Projects.GetProjects(_httpAuthenticator, folderName, projectGroupId);
		    return response == null 
				? new EnviromentMessage(null, $"No projects found at group {projectGroupId}", EnviromentMessageType.Error) 
				: new EnviromentMessage(response, "Succefully got projects.", EnviromentMessageType.Success);
	    }

		protected EnviromentMessage AddProjectGroup(Settings settings, HeaderBlockInfo headerInfo)
        {
	        if (headerInfo.ProjectGroupName == null)
	        {
				return new EnviromentMessage(null, "Using default project.", EnviromentMessageType.Warning);
			}				

			if (settings.Overwrite)
			{
				var response = GetProjectGroupItems();
				if (response.MessageType != EnviromentMessageType.Error)
				{
					var projectGroupResponse = response.MultipleContent.GetByName(headerInfo.ProjectGroupName);
					if (projectGroupResponse != null)
					{
						return new EnviromentMessage(response, "Group already exists.",
							EnviromentMessageType.Success);
					}
				}				
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
			return projectGroup == null
				? new EnviromentMessage(null, $"Failed to add {headerInfo.ProjectGroupName} project group", EnviromentMessageType.Error)
				: new EnviromentMessage(projectGroup, $"Project group {projectGroup.Name} added.", EnviromentMessageType.Success);
		}

        protected EnviromentMessage AddProject(Settings settings, HeaderBlockInfo headerInfo, string projectGroupId)
        {
	        if (settings.Overwrite)
	        {
		        var response = GetProjectItems(projectGroupId, headerInfo.FolderName);
		        if (response.MessageType != EnviromentMessageType.Error)
		        {
			        var projectResponse = response.MultipleContent.GetByName(headerInfo.ProjectName);
			        if (projectResponse != null)
			        {
						return new EnviromentMessage(projectResponse, "Project already exists.",
							EnviromentMessageType.Success);
					}
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
				return new EnviromentMessage(null,
					$"Project type {headerInfo.ProjectTypeName} doesn't exist. Project name: {headerInfo.ProjectName}",
					EnviromentMessageType.Error);
			}

	        var responsible = ApiRouter.Responsibles.GetResponsibles(_httpAuthenticator)
		        .GetByName(headerInfo.ResponsibleName.Replace(".", ""));
	        if (responsible == null)
	        {
		        return new EnviromentMessage(null,
			        $"Responsible {headerInfo.ResponsibleName} doesn't exist. Project name: {headerInfo.ProjectName}",
			        EnviromentMessageType.Error);
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
			return project == null
				? new EnviromentMessage(null, $"Failed to add {headerInfo.ProjectName} project", EnviromentMessageType.Error)
				: new EnviromentMessage(project, $"Project {project.Name} added.", EnviromentMessageType.Success);

		}

        protected async Task<EnviromentMessage> AddInformationAsync(string projectId, BlockInfo blockInfo,
            IDictionary<int, string> excelRow, int order)
        {
            var contentLines = new List<BlockLineInfo>();
            if (blockInfo.Lines == null || !blockInfo.Lines.Any())
                return new EnviromentMessage(null, "Block skipped.", EnviromentMessageType.Warning);

            var messageBuilder = new StringBuilder();
	        var resultMessageType = EnviromentMessageType.Success;
	        
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
	                    resultMessageType = EnviromentMessageType.Error;
					}
                }
                if (!contentFields.Any()) continue;
                var newLine = line.CloneWithFields(contentFields);
                contentLines.Add(newLine);
            }

            if (contentLines.All(c => !c.Fields.Any()))
                return new EnviromentMessage(null, "Block skipped.", EnviromentMessageType.Warning);

            var contentBlock = new
            {
                VisualBlockId = blockInfo.Id,
                ProjectId = projectId,
                Lines = contentLines,
                FrontOrder = order,
				Order = order
			};

	        var isSuccessResponse = await ApiRouter.ProjectCustomValues.Create(_httpAuthenticator, contentBlock);

	        string message;
	        if (isSuccessResponse)
	        {
		        message = $"Block created. {messageBuilder}";
	        }
	        else
	        {
		        message = $"Failed to add information block {blockInfo.Name}. {messageBuilder}";
		        resultMessageType = EnviromentMessageType.Error;
			}
	        return new EnviromentMessage(null, message, resultMessageType);
        }
    }
}