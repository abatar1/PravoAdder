using PravoAdder.Api.Domain;
using PravoAdder.Domain;

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
