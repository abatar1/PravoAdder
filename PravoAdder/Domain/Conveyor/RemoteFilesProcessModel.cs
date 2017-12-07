using System;
using System.Collections.Generic;
using System.Linq;
using PravoAdder.Api.Domain;

namespace PravoAdder.Domain
{
	public class RemoteFilesProcessModel
	{
		public EngineMessage Message { get; set; }
		public IEnumerable<VirtualCatalogItem> Context { get; set; }
		public string SearchingKey { get; set; }
		public ICollection<VisualBlockLine> Multilines { get; set; }

		private static VisualBlockModel _searchingBlockMetadata;
		public VisualBlockModel SearchingBlockMetadata
		{
			get
			{
				if (_searchingBlockMetadata == null)
				{
					_searchingBlockMetadata = ApiRouter.ProjectCustomValues.GetAllVisualBlocks(Message.Authenticator, Message.Item.Id)
						.MetadataOfBlocks.FirstOrDefault(x => x.Name.Equals(SearchingKey, StringComparison.InvariantCultureIgnoreCase));
				}
				return _searchingBlockMetadata;
			}
		}	
	}
}
