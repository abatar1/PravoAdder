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
				var projectGroupResponse = ProjectGroupRepository.Get(_httpAuthenticator, headerInfo.ProjectGroup);
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
		    return ProjectFolderRepository.GetOrCreate(authenticator,
			    headerInfo.ProjectFolder, new ProjectFolder {Name = folderName});
	    }

	    public static ProjectType GetProjectType(HttpAuthenticator authenticator, HeaderBlockInfo headerInfo, int count, bool createNewType = false)
	    {
		    var projectType = ProjectTypeRepository.Get(authenticator, headerInfo.ProjectType);

		    if (projectType == null)
		    {
				if (!createNewType)
				{
					Logger.Error(
						$"{DateTime.Now} | {count} | Project type {headerInfo.ProjectType} doesn't exist. Project name: {headerInfo.Name}");
					return null;
				}
			    var newType = new ProjectType
			    {
				    Name = headerInfo.ProjectType,
				    Abbreviation = headerInfo.ProjectType.SmartRemove(10)
			    };
			    projectType = ProjectTypeRepository.Create(authenticator, newType);
		    }

		    if (projectType.IsArchive)
		    {
				projectType = ProjectTypeRepository.GetDetailed(authenticator, headerInfo.ProjectType);
			    projectType.IsArchive = false;
				projectType = ApiRouter.ProjectTypes.Update(authenticator, projectType);
		    }

		    return projectType;
	    }

	    public Responsible GetResponsible(HttpAuthenticator authenticator, HeaderBlockInfo headerInfo, int count)
	    {
			var responsibleName = headerInfo.Responsible?.Replace(".", "");
		    if (string.IsNullOrEmpty(responsibleName)) responsibleName = "empty_string";

		    var responsible = ResponsibleRepository.Get(_httpAuthenticator, responsibleName);
		    if (responsible == null)
		    {
			    Logger.Error(
				    $"{DateTime.Now} | {count} | Responsible {responsibleName} doesn't exist. Project name: {headerInfo.Name}");
			    return null;
		    }
		    return responsible;
	    }

        public Project AddProject(Settings settings, HeaderBlockInfo headerInfo, string projectGroupId, int count)
        {						
	        if (string.IsNullOrEmpty(headerInfo.Name)) headerInfo.Name = "Default project name";

	        var projectFolder = TryCreateProjectFolder(_httpAuthenticator, headerInfo);

	        var projectType = GetProjectType(_httpAuthenticator, headerInfo, count, settings.CreateNewPracticeArea);
	        if (projectType == null) return null;

	        var responsible = GetResponsible(_httpAuthenticator, headerInfo, count);

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
				ProjectFolder = projectFolder,
				Description = headerInfo.Description,
			};

	        var project = settings.IsOverwrite
		        ? ProjectRepository.GetOrCreate(_httpAuthenticator, headerInfo.Name, newProject)
		        : ProjectRepository.Create(_httpAuthenticator, newProject);
	        if (project == null)
	        {
		        Logger.Error($"{DateTime.Now} | {count} | Failed to add {headerInfo.Name} project");
		        return null;
	        }

			if (!string.IsNullOrEmpty(headerInfo.Client))
	        {
				var newClient = new Participant(_httpAuthenticator, headerInfo.Client, ' ');
		        project.Client = ParticipantsRepository.GetOrCreate(_httpAuthenticator, headerInfo.Client, newClient);
				project = ApiRouter.Projects.Put(_httpAuthenticator, project);
			}

	        if (headerInfo.IsArchive)
	        {
		        ApiRouter.Projects.Archive(_httpAuthenticator, project.Id);
	        }
			
			return project;
		}

	    private VisualBlock CreateVisualBlockRequest(VisualBlockModel visualBlock, Row excelRow, string projectId, int order, VisualBlockModel projectVisualBlock = null)
	    {
			var contentLines = new List<VisualBlockLine>();
		    if (visualBlock.Lines == null || !visualBlock.Lines.Any()) return null;

		    var messageBuilder = new StringBuilder();
		    
		    foreach (var line in visualBlock.Lines)
		    {
			    var contentFields = new List<VisualBlockField>();
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

					    contentFields.Add((VisualBlockField) newFieldInfo);
					    continue;
				    }
				    var fieldData = excelRow[fieldInfo.ColumnNumber].Value;

				    if (string.IsNullOrEmpty(fieldData)) continue;

				    try
				    {
					    var value = FieldBuilder.CreateFieldValueFromData(_httpAuthenticator, fieldInfo, fieldData);
					    var newFieldInfo = fieldInfo.CloneWithValue(value);

					    contentFields.Add((VisualBlockField) newFieldInfo);
				    }
				    catch (Exception e)
				    {
					    messageBuilder.AppendLine(
						    $"Error while reading value from table! Message: {e.Message} Id: {projectId} Data: {fieldData}");
				    }
			    }
			    if (!contentFields.Any()) continue;

			    var newLine = (VisualBlockLine) line.CloneJson();
			    newLine.Values = new List<VisualBlockField>(contentFields);

			    newLine.Id = projectVisualBlock?.Lines.FirstOrDefault(x => x.BlockLineId.Equals(line.BlockLineId))?.Id;

				contentLines.Add(newLine);
		    }

		    if (contentLines.All(c => !c.Values.Any())) return null;

		    return new VisualBlock
		    {
			    VisualBlockId = visualBlock.Id,
			    ProjectId = projectId,
			    Lines = new List<VisualBlockLine>(contentLines),
			    FrontOrder = order,
			    Order = order,
				Message = messageBuilder.ToString().Trim(),
				Id = projectVisualBlock?.Id
			};
		}

        public bool AddInformation(VisualBlockModel visualBlock, Row excelRow, string projectId, int order)
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

	    public bool UpdateInformation(VisualBlockModel visualBlock, Row excelRow, string projectId, int order, VisualBlockModel projectVisualBlock = null)
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