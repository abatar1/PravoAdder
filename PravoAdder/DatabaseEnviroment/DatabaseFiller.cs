using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PravoAdder.Domain;
using PravoAdder.Domain.Info;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;

namespace PravoAdder.DatabaseEnviroment
{
    public class DatabaseFiller
    {
		private readonly HttpAuthenticator _httpAuthenticator;
	    private readonly FieldBuilder _fieldBuilder;

        public DatabaseFiller(HttpAuthenticator httpAuthenticator)
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

        protected EnviromentMessage AddProjectGroup(Settings settings, HeaderBlockInfo headerInfo)
        {
	        if (headerInfo.ProjectGroupName == null)
	        {
				return new EnviromentMessage(null, "Using default project.", EnviromentMessageType.Warning);
			}				

	        ProjectGroup projectGroup;
			if (settings.Overwrite)
			{
				projectGroup = ApiRouter.ProjectGroups.GetProjectGroups(_httpAuthenticator)
					.GetByName(headerInfo.ProjectGroupName);
				if (projectGroup != null)
				{
					return new EnviromentMessage(projectGroup, "Group already exists.",
						EnviromentMessageType.Success);
				}                   
            }

	        var projectFolder = ApiRouter.ProjectFolders.GetProjectFolders(_httpAuthenticator)
		        .GetByName(headerInfo.FolderName);
	        if (projectFolder == null)
	        {
				return new EnviromentMessage(null, "Project folder doesn't exist.", EnviromentMessageType.Error);
			}               

            var content = new
            {
                Name = headerInfo.ProjectGroupName,
                ProjectFolder = projectFolder,
                headerInfo.Description
            };

	        projectGroup = ApiRouter.ProjectGroups.ProjectGroups(_httpAuthenticator, content);
			return projectGroup == null
				? new EnviromentMessage(null, $"Failed to add {headerInfo.ProjectGroupName} project group", EnviromentMessageType.Error)
				: new EnviromentMessage(projectGroup, $"Project group {projectGroup.Name} added.", EnviromentMessageType.Success);
		}

        protected EnviromentMessage AddProject(Settings settings, HeaderBlockInfo headerInfo,
            string projectGroupId)
        {
	        Project project;
            if (settings.Overwrite)
            {
	            project = ApiRouter.Projects.GetProjects(_httpAuthenticator, headerInfo.FolderName, projectGroupId)
		            .GetByName(headerInfo.ProjectName);
	            if (project != null)
	            {
					return new EnviromentMessage(project, "Project already exists.", EnviromentMessageType.Success);
				}                  
            }

	        if (string.IsNullOrEmpty(headerInfo.ProjectName)) headerInfo.ProjectName = "Название проекта по-умолчанию";

			var projectFolder = ApiRouter.ProjectFolders.GetProjectFolders(_httpAuthenticator)
		        .GetByName(headerInfo.FolderName);
	        if (projectFolder == null)
	        {
				return new EnviromentMessage(null,
					$"Project folder {headerInfo.FolderName} doesn't exist. Project name: {headerInfo.ProjectName}",
					EnviromentMessageType.Error);
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

			var content = new
            {
                ProjectFolder = projectFolder,
                ProjectType = projectType,
                Responsible = responsible,
                ProjectGroup = projectGroup,
                Name = headerInfo.ProjectName
            };

	        project = ApiRouter.Projects.CreateProject(_httpAuthenticator, content);
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