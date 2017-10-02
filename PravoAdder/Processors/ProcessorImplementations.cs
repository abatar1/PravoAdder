using System;
using PravoAdder.Api.Domain;

namespace PravoAdder.Processors
{
	public class ProcessorImplementations
	{
		public static Func<EngineRequest, EngineRequest> AddProjectProcessor = request =>
		{
			var headerBlock = request.BlockReader.ReadHeader(request.ExcelRow);
			if (headerBlock == null) return null;	

			var projectGroup = request.ApiEnviroment.AddProjectGroup(request.Settings, headerBlock);
			var project = request.ApiEnviroment.AddProject(request.Settings, headerBlock, projectGroup?.Id);

			return string.IsNullOrEmpty(project?.Id) ? null : new EngineRequest
			{
				HeaderBlock = headerBlock,
				Item = (Project) project
			};
		};
	}
}
