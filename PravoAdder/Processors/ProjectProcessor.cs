using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fclp.Internals.Extensions;
using PravoAdder.Api;
using PravoAdder.Api.Domain;
using PravoAdder.Api.Repositories;
using PravoAdder.Domain;
using PravoAdder.Wrappers;

namespace PravoAdder.Processors
{
	public class ProjectProcessor
	{
		public Func<EngineMessage, EngineMessage> Update = message =>
		{
			message.Item = ProjectRepository.Get<ProjectsApi>(message.Authenticator, message.HeaderBlock.Name);
			return message;
		};

		public Func<EngineMessage, EngineMessage> TryCreate = message =>
		{
			var headerBlock = message.HeaderBlock;
			if (string.IsNullOrEmpty(headerBlock.Name) || string.IsNullOrEmpty(headerBlock.ProjectType)) return null;

			var projectGroup = message.ApiEnviroment.AddProjectGroup(message.Args.IsOverwrite, headerBlock);
			message.Item = message.ApiEnviroment.AddProject(message.Args.IsOverwrite, headerBlock,
				projectGroup?.Id, message.Count, message.IsUpdate);

			return message;
		};

		public Func<EngineMessage, EngineMessage> Delete = message =>
		{
			message.ApiEnviroment.DeleteProjectItem(message.Item.Id);
			return message;
		};

		public Func<EngineMessage, EngineMessage> Rename = message =>
		{
			var projectName = Table.GetValue(message.Table.Header, message.Row, "Case name");
			var project = ProjectRepository.Get<ProjectsApi>(message.Authenticator, projectName);
			if (project == null) return null;

			project.Name = Table.GetValue(message.Table.Header, message.Row, "New case name");
			message.Item = ApiRouter.Projects.Put(message.Authenticator, project);
			return message;
		};

		public Func<EngineMessage, EngineMessage> AddNote = message =>
		{
			var projectName = message.GetValueFromRow("Case Name");

			var project = ProjectRepository.Get<ProjectsApi>(message.Authenticator, projectName);
			if (project == null) return null;

			var note = new Note
			{
				Project = project,
				Text = message.GetValueFromRow("Note"),
				IsPrivate = bool.Parse(message.GetValueFromRow("Is Private"))
			};
			ApiRouter.Notes.Create(message.Authenticator, note);
			return new EngineMessage { Item = project };
		};

		public Func<EngineMessage, EngineMessage> Synchronize = message =>
		{
			if (!string.IsNullOrEmpty(message.HeaderBlock.CasebookNumber))
			{			
				var project = ProjectRepository.Get<ProjectsApi>(message.Authenticator, message.HeaderBlock.Name);
				if (project == null) return null;

				var asyncResult = ApiRouter.Casebook.CheckAsync(message.Authenticator, project.Id,
					message.HeaderBlock.CasebookNumber).Result;
				message.Item = project;
				return message;
			}
			return null;
		};

		public Func<EngineMessage, EngineMessage> AddInformation = message =>
		{
			if (message.Item == null) return message;

			var blocksInfo = message.CaseBuilder.Build(((Project) message.Item).ProjectType);
			if (blocksInfo == null) return null;

			var projectId = message.Item.Id;			

			foreach (var repeatBlock in blocksInfo)
			{
				foreach (var blockInfo in repeatBlock.VisualBlocks)
				{
					var projectVisualBlock = ApiRouter.ProjectCustomValues.GetAllVisualBlocks(message.Authenticator, projectId)
						.Blocks.First(x => x.Name.Equals(blockInfo.NameInConstructor));
					message.ApiEnviroment.AddInformation(blockInfo, message.Row, projectId, repeatBlock.Order);
				}
			}
			return message;
		};

		public Func<EngineMessage, EngineMessage> UpdateInformation = message =>
		{
			if (message.Item == null) return message;

			var projectType = ((Project) message.Item).ProjectType;
			var blocksInfo = message.CaseBuilder.Build(projectType);
			if (blocksInfo == null) return null;

			var projectId = message.Item.Id;

			var successFlag = false;
			foreach (var repeatBlock in blocksInfo)
			{
				foreach (var blockInfo in repeatBlock.VisualBlocks)
				{
					var blockName = blockInfo.Name;
					if (blockInfo.Name.Contains("-"))
					{
						blockName = blockInfo.Name.Remove(blockInfo.Name.IndexOf("-", StringComparison.Ordinal)).Trim();
					}

					var projectVisualBlocks = ApiRouter.ProjectCustomValues.GetAllVisualBlocks(message.Authenticator, projectId)
						.Blocks.Where(x => x.Name.Contains(blockName)).ToList();

					if (projectVisualBlocks.Count > 0)
					{
						projectVisualBlocks
							.Skip(1)
							.ForEach(x => ApiRouter.ProjectCustomValues.Delete(message.Authenticator, x.Id));

						var processingBlock = projectVisualBlocks.First();
						if (message.ApiEnviroment.UpdateInformation(blockInfo, message.Row, message.Item.Id, repeatBlock.Order,
							processingBlock))
						{
							successFlag = true;
						}							
					}
					else
					{
						if (message.ApiEnviroment.AddInformation(blockInfo, message.Row, message.Item.Id, repeatBlock.Order))
						{
							successFlag = true;
						}
					}					
				}
			}
			return !successFlag ? null : message;
		};

		public Func<EngineMessage, EngineMessage> CreateProjectField = message =>
		{
			var projectField = (ProjectField) message.GetCreatable();
			if (projectField == null) return null;

			var result = ApiRouter.ProjectFields.Create(message.Authenticator, projectField);
			message.Item = result;
			return message;
		};

		public Func<EngineMessage, EngineMessage> SetClient = message =>
		{
			var project = ProjectRepository.GetDetailed<ProjectsApi>(message.Authenticator, message.GetValueFromRow("Case Name"));
			if (project == null) return null;

			if (project.Client == null)
			{
				var clientName = message.GetValueFromRow("Client");
				if (string.IsNullOrEmpty(clientName)) return null;

				try
				{
					var client = new Participant(clientName, ' ', ParticipantType.GetPersonType(message.Authenticator));
					project.Client = ParticipantsRepository.GetOrCreate<ParticipantsApi>(message.Authenticator, clientName, client);
				}
				catch (Exception)
				{
					return null;
				}
				project = ApiRouter.Projects.Put(message.Authenticator, project);
			}
			message.Item = project;

			return message;
		};

		public Func<EngineMessage, EngineMessage> CreateType = message =>
		{
			var typeName = message.GetValueFromRow("Name");
			if (string.IsNullOrEmpty(typeName)) return null;
			var abbreviation = message.GetValueFromRow("Abbreviation");
			if (string.IsNullOrEmpty(abbreviation)) return null;

			var processingType = ProjectTypeRepository.GetDetailedOrPut(message.Authenticator, typeName, abbreviation);		

			var blockName = message.GetValueFromRow("Blocks");
			var block = VisualBlockRepository.Get<VisualBlockApi>(message.Authenticator, blockName);
			if (block == null) return null;

			if (processingType.VisualBlocks == null) processingType.VisualBlocks = new List<VisualBlockModel>();
			if (processingType.VisualBlocks.Contains(block)) return null;

			processingType.VisualBlocks.Add(block);
			var updatedType = ApiRouter.ProjectTypes.Create(message.Authenticator, processingType);

			message.Item = updatedType;
			return message;
		};

		public Func<EngineMessage, EngineMessage> AttachParticipant = message =>
		{
			var projectName = message.GetValueFromRow("Case Name");
			var project = ProjectRepository.Get<ProjectsApi>(message.Authenticator, projectName);
			if (project == null) return null;

			var participantName = message.GetValueFromRow("Participant");
			var detailedParticipant =
				ParticipantsRepository.GetDetailed<ParticipantsApi>(message.Authenticator, participantName);
			if (detailedParticipant == null) return null;

			detailedParticipant.IncludeInProjectId = project.Id;

			ApiRouter.Participants.Create(message.Authenticator, detailedParticipant);
			message.Item = detailedParticipant;
			return message;
		};

		private static List<DictionaryItem> _calculationTypes;

		public Func<EngineMessage, EngineMessage> UpdateSettings = message =>
		{
			var projectName = message.GetValueFromRow("Case Name");
			var project = ProjectRepository.Get<ProjectsApi>(message.Authenticator, projectName);
			if (project == null) return null;

			var billingRules = (BillingRuleWrapper) message.GetCreatable(project);

			var projectSettings = ApiRouter.ProjectSettings.Get(message.Authenticator, project.Id);
			if (_calculationTypes == null)
				_calculationTypes = ApiRouter.DefaultDictionaryItems.GetMany(message.Authenticator, "CalculationTypes");

			projectSettings.BillingRules.AddRange(billingRules.BillingRules);
			ApiRouter.ProjectSettings.Save(message.Authenticator, projectSettings);

			message.Item = project;
			return message;
		};

		private static List<DictionaryItem> _documentTypes;

		private static void ProcessLocalFiles(EngineMessage message, IReadOnlyCollection<VirtualCatalogItem> context, DirectoryInfo directoryInfo, string parentId)
		{
			var bulks = new List<Bulk>();
			foreach (var subFile in directoryInfo.GetFiles())
			{							
				var file = ApiRouter.Upload.Upload(message.Authenticator, subFile).Result;
				if (file == null) continue;
				var documentType = _documentTypes.FirstOrDefault(x => x.Name == "Draft");

				var bulk = new Bulk
				{
					DocumentFolderId = parentId,
					DocumentType = documentType,
					File = file,
					ReceivedDate = DateTime.Today,
					DocumentContentType = new {file.Name}
				};
				message.Counter.ProcessCount(message.Count, message.Total, message.Args.RowNum, file);

				bulks.Add(bulk);
			}

			if (bulks.Count > 0)
			{
				ApiRouter.Documents.Create(message.Authenticator, bulks);
				message.Counter.Message($"{bulks.Count} files succesfuly uploaded from {directoryInfo.Name}");
			}			

			foreach (var subDirectory in directoryInfo.GetDirectories())
			{
				var folderName = subDirectory.Name;
				var folder = context.FirstOrDefault(f => f.Name.Equals(folderName));
				if (folder == null)
				{
					folder = ApiRouter.DocumentFolders.Create(message.Authenticator,
						new VirtualCatalogItem
						{
							ParentId = parentId,
							Name = folderName,
							SysName = "Folder"
						});
				}				
				context = ApiRouter.VirtualCatalog.GetContext(message.Authenticator, folder.Id);
				ProcessLocalFiles(message, context, subDirectory, folder.Id);
			}
		}

		private static void ProcessRemoteFiles(RemoteFilesProcessModel processModel)
		{
			foreach (var catalogItem in processModel.Context)
			{
				if (!catalogItem.Name.Equals(processModel.SearchingKey, StringComparison.InvariantCultureIgnoreCase)) continue;

				var subContext = ApiRouter.VirtualCatalog.GetContext(processModel.Message.Authenticator, catalogItem.Id);

				foreach (var file in subContext.Where(x => x.SysName == "Document"))
				{
					var blockLine = processModel.SearchingBlockMetadata.Lines
						.First(line => line.Fields.Any(field => field.IsTypeOf("Document")));
					var newLine = new VisualBlockLine
					{
						Order = processModel.Multilines.Count,
						BlockLineId = blockLine.Id,
						Values = new List<VisualBlockField>
						{
							new VisualBlockField
							{
								VisualBlockProjectFieldId = blockLine.Fields.First(field => field.IsTypeOf("Document")).Id,
								Value = new
								{
									file.Id,
									file.Name
								}
							}
						}
					};
					processModel.Multilines.Add(newLine);
				}

				foreach (var subFolder in subContext.Where(x => x.SysName == "Folder"))
				{
					processModel.Context = ApiRouter.VirtualCatalog.GetContext(processModel.Message.Authenticator, subFolder.Id);
					ProcessRemoteFiles(processModel);
				}
			}
		}

		public Func<EngineMessage, EngineMessage> DocumentsToMultilines = message =>
		{
			if (_documentTypes == null)
				_documentTypes = ApiRouter.DefaultDictionaryItems.GetMany(message.Authenticator, "CaseMap.Modules.Documents.DAL.Data.DocumentType");
			
			var searchingFolder = message.Args.SearchKey;

			var project = ProjectRepository.GetDetailed<ProjectsApi>(message.Authenticator, message.Item.Name);
			if (!project.Name.Equals("Mary-MVA-2014-145")) return null;

			var searchingBlockMetadata = ApiRouter.ProjectCustomValues.GetAllVisualBlocks(message.Authenticator, project.Id)
				.MetadataOfBlocks.FirstOrDefault(x => x.Name.Equals(searchingFolder, StringComparison.InvariantCultureIgnoreCase));
			if (searchingBlockMetadata == null) return null;
			
			var context = ApiRouter.VirtualCatalog.GetContext(message.Authenticator, project.DocumentFolderId);
			var multilines = new List<VisualBlockLine>();

			var processModel = new RemoteFilesProcessModel
			{
				Context = context,
				Message = message,
				Multilines = multilines,
				SearchingKey = searchingFolder
			};
			ProcessRemoteFiles(processModel);

			var block = new VisualBlock
			{
				VisualBlockId = processModel.SearchingBlockMetadata.Id,
				Order = 0,
				FrontOrder = 0,
				Lines = processModel.Multilines.ToList(),
				ProjectId = project.Id
			};
			ApiRouter.ProjectCustomValues.Create(message.Authenticator, block);

			return message;
		};

		public Func<EngineMessage, EngineMessage> Files = message =>
		{
			if (_documentTypes == null)
				_documentTypes = ApiRouter.DefaultDictionaryItems.GetMany(message.Authenticator, "CaseMap.Modules.Documents.DAL.Data.DocumentType");

			var project = ProjectRepository.GetDetailed<ProjectsApi>(message.Authenticator, message.HeaderBlock.Name);
			var directoryInfo = new DirectoryInfo(message.HeaderBlock.FilesPath);
			var existingFolders = ApiRouter.VirtualCatalog.GetContext(message.Authenticator, project.DocumentFolderId);

			ProcessLocalFiles(message, existingFolders, directoryInfo, project.DocumentFolderId);

			return message;
		};
	}
}
