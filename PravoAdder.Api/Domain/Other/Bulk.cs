using System;

namespace PravoAdder.Api.Domain
{
	public class Bulk : DatabaseEntityItem
	{
		public FileResponse File { get; set; }
		public DictionaryItem DocumentType { get; set; }
		public string DocumentFolderId { get; set; }
		public DateTime ReceivedDate { get; set; }
		public object DocumentContentType { get; set; }
	}
}
