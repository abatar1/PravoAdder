﻿using System;

namespace PravoAdder.Processors
{
	public class ProcessorImplementations
	{
		public static Func<EngineRequest, EngineRequest> AddProjectProcessor = request =>
		{
			var headerBlock = request.BlockReader.ReadHeader(request.ExcelRow);
			if (headerBlock == null) return null;	

			var projectGroup = request.ApiEnviroment.AddProjectGroup(request.Settings, headerBlock);
			var project = request.ApiEnviroment.AddProject(request.Settings, headerBlock, projectGroup?.Id, request.Count);

			return string.IsNullOrEmpty(project?.Id) ? null : new EngineRequest
			{
				HeaderBlock = headerBlock,
				Item = project
			};
		};
	}
}
