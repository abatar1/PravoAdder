using System;
using PravoAdder.Api.Domain;

namespace PravoAdder.Processors
{
	public class ProcessorImplementations
	{
		public static Func<EngineRequest, EngineResponse> AddProjectProcessor = request =>
		{
			var headerBlock = request.BlockReader.ReadHeaderBlock(request.Row);
			if (headerBlock == null) return null;		

			var projectGroup = request.Migrator.AddProjectGroup(headerBlock);
			var project = request.Migrator.AddProject(headerBlock, projectGroup?.Id);

			return string.IsNullOrEmpty(project?.Id) ? null : new EngineResponse
			{
				HeaderBlock = headerBlock,
				Item = (Project) project
			};
		};
	}
}
