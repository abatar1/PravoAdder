using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Helpers;
using PravoAdder.Domain;
using PravoAdder.Helpers;

namespace PravoAdder.Wrappers
{
    public class ApiEnviroment
    {
	    private const int MaxWordLength = 350;
		private readonly HttpAuthenticator _httpAuthenticator;
	    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
	    private static List<Project> _projects;

		public ApiEnviroment(HttpAuthenticator httpAuthenticator)
        {
            _httpAuthenticator = httpAuthenticator;
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

	    public List<ProjectGroup> GetProjectGroupItems()
	    {
		    var response = ApiRouter.ProjectGroups.GetProjectGroups(_httpAuthenticator);
		    response.Add(ProjectGroup.Empty);
			return response.ToList();
	    }

		public ProjectGroup AddProjectGroup(bool needOverwrite, HeaderBlockInfo headerInfo)
        {
	        if (string.IsNullOrEmpty(headerInfo.ProjectGroup)) return null;		

			if (needOverwrite)
			{
				var response = GetProjectGroupItems();
				var projectGroupResponse = response?.GetByName(headerInfo.ProjectGroup);
				if (projectGroupResponse != null) return projectGroupResponse;
			}

	        var projectFolder = ApiRouter.ProjectFolders.GetProjectFolders(_httpAuthenticator)
		        .GetByName(headerInfo.ProjectFolder);
	        if (projectFolder == null)
	        {
		        ApiRouter.ProjectFolders.InsertProjectFolder(headerInfo.ProjectFolder, _httpAuthenticator);
			}               

            var content = new
            {
                Name = headerInfo.ProjectGroup,
                ProjectFolder = projectFolder,
                headerInfo.Description
            };

	        var projectGroup = ApiRouter.ProjectGroups.ProjectGroups(_httpAuthenticator, content);
			if (projectGroup == null) Logger.Error($"Failed to add {headerInfo.ProjectGroup} project group");
	        return projectGroup;			
		}

        public Project AddProject(bool needOverwrite, HeaderBlockInfo headerInfo, string projectGroupId, int count, bool isUpdate)
        {
	        if (isUpdate || needOverwrite)
	        {
		        if (isUpdate)
		        {
			        if (_projects == null)
			        {
				        _projects = ApiRouter.Projects.GetProjects(_httpAuthenticator, headerInfo.ProjectFolder);					
			        }
			        var projectResponse = _projects.GetByName(headerInfo.Name);
			        if (projectResponse != null) return projectResponse;
				}
		        else
		        {
			        var projects = ApiRouter.Projects.GetProjects(_httpAuthenticator, headerInfo.ProjectFolder);
			        var projectResponse = projects.GetByName(headerInfo.Name);
			        if (projectResponse != null) return projectResponse;
				}		       
            }

	        if (string.IsNullOrEmpty(headerInfo.Name)) headerInfo.Name = "Название проекта по-умолчанию";

			var projectFolder = ApiRouter.ProjectFolders.GetProjectFolders(_httpAuthenticator)
		        .GetByName(headerInfo.ProjectFolder);

	        if (projectFolder == null)
	        {
		        var folderName = headerInfo.ProjectFolder;

		        if (folderName.Length > MaxWordLength)
					folderName = folderName.Remove(MaxWordLength);
		        projectFolder = ApiRouter.ProjectFolders.InsertProjectFolder(folderName, _httpAuthenticator);
			}

	        var projectType = ApiRouter.ProjectTypes.GetProjectTypes(_httpAuthenticator)
		        .GetByName(headerInfo.ProjectType);
	        if (projectType == null)
	        {
		        Logger.Error(
			        $"{DateTime.Now} | {count} | Project type {headerInfo.ProjectType} doesn't exist. Project name: {headerInfo.Name}");
		        return null;
	        }

	        var responsibleName = headerInfo.Responsible?.Replace(".", "");
	        if (string.IsNullOrEmpty(responsibleName)) responsibleName = "empty_string";
	        var responsible = ApiRouter.Responsibles.GetResponsibles(_httpAuthenticator).GetByName(responsibleName);
	        if (responsible == null)
	        {
		        Logger.Error(
			        $"{DateTime.Now} | {count} | Responsible {responsibleName} doesn't exist. Project name: {headerInfo.Name}");
		        return null;
	        }

	        var projectGroup = string.IsNullOrEmpty(headerInfo.ProjectGroup) && projectGroupId == null
		        ? null
		        : new ProjectGroup(headerInfo.ProjectGroup, projectGroupId);

			var projectName = headerInfo.Name;
	        if (projectName.Length > MaxWordLength) projectName = projectName.Remove(MaxWordLength);

	        var newProject = new Project
	        {
		        CasebookNumber = headerInfo.CasebookNumber,
		        Name = projectName,
				ProjectType = projectType,
				Responsible = responsible,
				ProjectGroup = projectGroup,
				ProjectFolder = projectFolder				
			};

	        var project = ApiRouter.Projects.CreateProject(_httpAuthenticator, newProject);
			if (project == null) Logger.Error($"{DateTime.Now} | Failed to add {headerInfo.Name} project");
			return project;
		}

        public void AddInformation(BlockInfo blockInfo, Row excelRow, string projectId, int order)
        {		
			var contentLines = new List<BlockLineInfo>();
	        if (blockInfo.Lines == null || !blockInfo.Lines.Any()) return;

            var messageBuilder = new StringBuilder();	        
			var calculationIds = new List<string>();
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
                    var fieldData = excelRow[fieldInfo.ColumnNumber].Value;

                    if (string.IsNullOrEmpty(fieldData)) continue;

                    try
                    {
                        var value = FieldBuilder.CreateFieldValueFromData(_httpAuthenticator, fieldInfo, fieldData);
	                    if (value is CalculationFormulaValue)
	                    {
							calculationIds.Add(((CalculationFormulaValue) value).CalculationFormulaId);
						}

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

	        var visualBlockResponse = ApiRouter.ProjectCustomValues.Create(_httpAuthenticator, contentBlock);

			//if (calculationIds.Count > 0)
			//{
			//	bool IsCorrectFormula(BlockFieldInfo field, string calculationId) => field.Type == "CalculationFormula" &&
			//																		 ((dynamic)field.Value)
			//																		 .CalculationFormulaId == calculationId;
			//	var lines = new List<BlockLineInfo>();
			//	foreach (var calculationId in calculationIds)
			//	{
			//		var formulaLine =
			//			contentLines.First(line => line.Fields.Any(field => IsCorrectFormula(field, calculationId)));
			//		formulaLine.Id = visualBlockResponse.Lines.First(line => line.BlockLineId == formulaLine.BlockLineId).Id;

			//		var formulaField =
			//			formulaLine.Fields.First(field => IsCorrectFormula(field, calculationId));
			//		formulaField.Value = new
			//		{
			//			((dynamic)formulaField.Value).CalculationFormulaId,
			//			Result = ApiRouter.CalculationFormulas.GetInputData(_httpAuthenticator, projectId, calculationId,
			//				blockInfo.Id, formulaLine.BlockLineId)
			//		};

			//		var existedLine = lines.FirstOrDefault(line => line.BlockLineId == formulaLine.BlockLineId);
			//		if (existedLine == null)
			//		{
			//			formulaLine.Fields = new List<BlockFieldInfo> { formulaField };
			//			lines.Add(formulaLine);
			//		}
			//		else
			//		{
			//			lines.First(line => line.BlockLineId == formulaLine.BlockLineId).Fields.Add(formulaField);
			//		}
			//	}
			//	var updateContentBlock = new
			//	{
			//		visualBlockResponse.Id,
			//		VisualBlockId = blockInfo.Id,
			//		ProjectId = projectId,
			//		Lines = lines,
			//		FrontOrder = order,
			//		Order = order
			//	};
			//	var isSuccessUpdateResponse = ApiRouter.ProjectCustomValues.Update(_httpAuthenticator, updateContentBlock);
			//}
			if (visualBlockResponse == null)
			{
				Logger.Error(
					$"{DateTime.Now} | Failed to create information block {blockInfo.Name}. {messageBuilder.ToString().Trim()} | Id : {projectId}");
			}
		}
    }
}