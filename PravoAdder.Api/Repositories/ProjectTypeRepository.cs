using System.Collections.Generic;
using PravoAdder.Api.Domain;

namespace PravoAdder.Api.Repositories
{
	public class ProjectTypeRepository : TemplateRepository<ProjectType>
	{
		public static ProjectType GetDetailedOrPut(HttpAuthenticator authenticator, string typeName, string abbreviation)
		{
			if (string.IsNullOrEmpty(typeName) || string.IsNullOrEmpty(abbreviation)) return null;

			var projectType = GetDetailed<ProjectTypesApi>(authenticator, typeName);

			if (projectType == null)
			{
				var content = new ProjectType
				{
					Name = typeName,
					Abbreviation = abbreviation,
					VisualBlocks = new List<VisualBlockModel>()
				};
				projectType = ApiRouter.ProjectTypes.Create(authenticator, content);
				Container.AddOrUpdate(typeName, projectType, (key, type) => type);
			}
			return projectType;
		}
	}
}
