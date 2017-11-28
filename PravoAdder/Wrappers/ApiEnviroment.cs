using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Repositories;
using PravoAdder.Domain;
using PravoAdder.Helpers;

namespace PravoAdder.Wrappers
{
    public class ApiEnviroment
    {
	    private const int MaxWordLength = 350;
		private readonly HttpAuthenticator _httpAuthenticator;
	    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		public ApiEnviroment(HttpAuthenticator httpAuthenticator)
        {
            _httpAuthenticator = httpAuthenticator;
        }

	    public void DeleteProjectGroupItem(string projectGroupId)
	    {
		    try
		    {
			    ApiRouter.ProjectGroups.Archive(_httpAuthenticator, projectGroupId);
				ApiRouter.ProjectGroups.Delete(_httpAuthenticator, projectGroupId);			
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
			    ApiRouter.Projects.Archive(_httpAuthenticator, projectId);
			    ApiRouter.Projects.Delete(_httpAuthenticator, projectId);
			}
		    catch (Exception e)
		    {
			    Logger.Error($"Project deleting failed. Reason: {e.Message}");
		    }
		}

		public ProjectGroup AddProjectGroup(bool needOverwrite, HeaderBlockInfo headerInfo)
        {
	        if (string.IsNullOrEmpty(headerInfo.ProjectGroup)) return null;		

			if (needOverwrite)
			{
				var projectGroupResponse = ProjectGroupRepository.Get<ProjectGroupsApi>(_httpAuthenticator, headerInfo.ProjectGroup);
				if (projectGroupResponse != null) return projectGroupResponse;
			}

	        var projectFolder = TryCreateProjectFolder(_httpAuthenticator, headerInfo);

			var newProjectGroup = new ProjectGroup
			{
                Name = headerInfo.ProjectGroup,
                ProjectFolder = projectFolder,
                Description = headerInfo.Description
            };

	        var projectGroup = ApiRouter.ProjectGroups.Create(_httpAuthenticator, newProjectGroup);
			if (projectGroup == null) Logger.Error($"Failed to add {headerInfo.ProjectGroup} project group");
	        return projectGroup;			
		}

	    public static ProjectFolder TryCreateProjectFolder(HttpAuthenticator authenticator, HeaderBlockInfo headerInfo)
	    {
		    var folderName = headerInfo.ProjectFolder.SliceSpaceIfMore(MaxWordLength);
		    return ProjectFolderRepository.GetOrCreate<ProjectFoldersApi>(authenticator,
			    headerInfo.ProjectFolder, new ProjectFolder {Name = folderName});
	    }

	    public static ProjectType GetProjectType(HttpAuthenticator authenticator, HeaderBlockInfo headerInfo, int count)
	    {
		    var projectType = ProjectTypeRepository.Get<ProjectTypesApi>(authenticator, headerInfo.ProjectType);
		    if (projectType == null)
		    {
			    Logger.Error(
				    $"{DateTime.Now} | {count} | Project type {headerInfo.ProjectType} doesn't exist. Project name: {headerInfo.Name}");
			    return null;
		    }
		    return projectType;
	    }

	    public Responsible GetResponsible(HttpAuthenticator authenticator, HeaderBlockInfo headerInfo, int count)
	    {
			var responsibleName = headerInfo.Responsible?.Replace(".", "");
		    if (string.IsNullOrEmpty(responsibleName)) responsibleName = "empty_string";

		    var responsible = ResponsibleRepository.Get<ResponsiblesApi>(_httpAuthenticator, responsibleName);
		    if (responsible == null)
		    {
			    Logger.Error(
				    $"{DateTime.Now} | {count} | Responsible {responsibleName} doesn't exist. Project name: {headerInfo.Name}");
			    return null;
		    }
		    return responsible;
	    }

        public Project AddProject(bool needOverwrite, HeaderBlockInfo headerInfo, string projectGroupId, int count, bool isUpdate)
        {			
			if (needOverwrite)
			{
				var projectResponse = ProjectRepository.Get<ProjectsApi>(_httpAuthenticator, headerInfo.Name);
			    if (projectResponse != null) return projectResponse;					       
            }

	        if (string.IsNullOrEmpty(headerInfo.Name)) headerInfo.Name = "Название проекта по-умолчанию";

	        var projectFolder = TryCreateProjectFolder(_httpAuthenticator, headerInfo);
	        var projectType = GetProjectType(_httpAuthenticator, headerInfo, count);

	        var responsible = GetResponsible(_httpAuthenticator, headerInfo, count);

			var projectGroup = string.IsNullOrEmpty(headerInfo.ProjectGroup) && projectGroupId == null
		        ? null
		        : new ProjectGroup(headerInfo.ProjectGroup, projectGroupId);

			var projectName = headerInfo.Name;
	        if (projectName.Length > MaxWordLength) projectName = projectName.Remove(MaxWordLength);

	        var newClient = new Participant(headerInfo.Client, ' ', ParticipantType.GetPersonType(_httpAuthenticator));
	        var client =
		        ParticipantsRepository.GetOrCreate<ParticipantsApi>(_httpAuthenticator, headerInfo.Client, newClient);

	        var newProject = new Project
	        {
		        CasebookNumber = headerInfo.CasebookNumber,
		        Name = projectName,
				ProjectType = projectType,
				Responsible = responsible,
				ProjectGroup = projectGroup,
				ProjectFolder = projectFolder,
				Description = headerInfo.Description,
				Client = client
			};

	        var project = ApiRouter.Projects.Create(_httpAuthenticator, newProject);
			if (project == null) Logger.Error($"{DateTime.Now} | {count} | Failed to add {headerInfo.Name} project");
			return project;
		}

	    private VisualBlockParticipant CreateVisualBlockRequest(VisualBlock visualBlock, Row excelRow, string projectId, int order, VisualBlock projectVisualBlock = null)
	    {
			var contentLines = new List<VisualBlockParticipantLine>();
		    if (visualBlock.Lines == null || !visualBlock.Lines.Any()) return null;

		    var messageBuilder = new StringBuilder();
		    
		    foreach (var line in visualBlock.Lines)
		    {
			    var contentFields = new List<VisualBlockParticipantField>();
			    foreach (var fieldInfo in line.Fields)
			    {
				    if (!excelRow.ContainsKey(fieldInfo.ColumnNumber))
				    {
					    messageBuilder.AppendLine($"Excel row doesn't contain \"{fieldInfo.ColumnNumber}\" key for \"{fieldInfo.ProjectField.PlaceholderText}\".");
					    continue;
				    }
				    var calculationFormula = fieldInfo.ProjectField.CalculationFormulas?[0];
				    if (calculationFormula != null)
				    {
					    var calcResult = ApiRouter.CalculationFormulas.GetInputData(_httpAuthenticator, calculationFormula.Id, visualBlock.Id,
						    line.BlockLineId, projectVisualBlock?.Id, projectId);
					    var newFieldInfo =
						    fieldInfo.CloneWithValue(
							    new CalculationFormulaValue { CalculationFormulaId = calculationFormula.Id, Result = calcResult });
					    contentFields.Add((VisualBlockParticipantField)newFieldInfo);
					    continue;
				    }
				    var fieldData = excelRow[fieldInfo.ColumnNumber].Value;

				    if (string.IsNullOrEmpty(fieldData)) continue;

				    try
				    {
					    var value = FieldBuilder.CreateFieldValueFromData(_httpAuthenticator, fieldInfo, fieldData);

					    var newFieldInfo = fieldInfo.CloneWithValue(value);
					    contentFields.Add((VisualBlockParticipantField)newFieldInfo);
				    }
				    catch (Exception e)
				    {
					    messageBuilder.AppendLine(
						    $"Error while reading value from table! Message: {e.Message} Id: {projectId} Data: {fieldData}");
				    }
			    }
			    if (!contentFields.Any()) continue;

			    var newLine = (VisualBlockParticipantLine) line.CloneJson();
			    newLine.Values = new List<VisualBlockParticipantField>(contentFields);
			    newLine.Id = projectVisualBlock?.Lines.FirstOrDefault(x => x.BlockLineId.Equals(line.BlockLineId))?.Id;
			    contentLines.Add(newLine);
		    }

		    if (contentLines.All(c => !c.Values.Any())) return null;

		    return new VisualBlockParticipant
		    {
			    VisualBlockId = visualBlock.Id,
			    ProjectId = projectId,
			    Lines = new List<VisualBlockParticipantLine>(contentLines),
			    FrontOrder = order,
			    Order = order,
				Message = messageBuilder.ToString().Trim(),
				Id = projectVisualBlock?.Id
			};
		}

        public bool AddInformation(VisualBlock visualBlock, Row excelRow, string projectId, int order)
        {
	        var contentBlock = CreateVisualBlockRequest(visualBlock, excelRow, projectId, order);
	        if (contentBlock == null) return false;

			var visualBlockResponse = ApiRouter.ProjectCustomValues.Create(_httpAuthenticator, contentBlock);
			
			if (visualBlockResponse == null)
			{
				Logger.Error(
					$"{DateTime.Now} | Failed to create information block {visualBlock.Name}. {contentBlock.Message} | Id : {projectId}");
				return false;
			}
	        return true;
		}

	    public bool UpdateInformation(VisualBlock visualBlock, Row excelRow, string projectId, int order, VisualBlock projectVisualBlock = null)
	    {
			var contentBlock = CreateVisualBlockRequest(visualBlock, excelRow, projectId, order, projectVisualBlock);
		    if (contentBlock == null) return false;

		    var visualBlockResponse = ApiRouter.ProjectCustomValues.Update(_httpAuthenticator, contentBlock);

		    if (!visualBlockResponse.Result)
		    {
			    Logger.Error(
				    $"{DateTime.Now} | Failed to update information block {visualBlock.Name}. {contentBlock.Message} | Id : {projectId}");
			    return false;
			}
		    return true;
		}
	}
}