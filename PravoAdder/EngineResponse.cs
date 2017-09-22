using PravoAdder.Api.Domain;
using PravoAdder.Domain.Info;

namespace PravoAdder
{
	public class EngineResponse
	{
		public EngineResponse(HeaderBlockInfo headerBlock, Project project)
		{
			HeaderBlock = headerBlock;
			Project = project;
		}

		public Project Project { get; }
		public HeaderBlockInfo HeaderBlock { get; }
	}
}
