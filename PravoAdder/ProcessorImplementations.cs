using System;
using PravoAdder.Api.Domain;

namespace PravoAdder
{
	public class ProcessorImplementations
	{
		public static Func<EngineRequest, EngineResponse> MigrationMasterDataProcessor = request =>
		{
			var headerBlock = request.BlockReader.ReadHeader(request.ExcelRow);
			if (headerBlock == null) return null;

			var projectGroup = request.Migrator.AddProjectGroup(headerBlock);
			var project = request.Migrator.AddProject(headerBlock, projectGroup?.Id);

			return string.IsNullOrEmpty(project?.Id) ? null : new EngineResponse(headerBlock, (Project) project);
		};

		public static Func<EngineRequest, EngineResponse> CleaningMasterDataProcessor = request =>
		{
			return null;
		};
	}
}
