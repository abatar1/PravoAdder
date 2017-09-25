using System;
using System.Collections;
using System.Collections.Generic;
using PravoAdder.Api.Domain;

namespace PravoAdder.Api
{
    public class EnviromentMessage
    {
        public EnviromentMessage(dynamic content, string message, EnviromentMessageType type)
        {
	        if (content is IList)
	        {
		        MultipleContent = new List<DatabaseEntityItem>((IEnumerable<DatabaseEntityItem>) content);
				ContentType = EnviromentContentType.Multiple;
	        }				
			else if (content is DatabaseEntityItem)
	        {
		        SingleContent = (DatabaseEntityItem) content;
				ContentType = EnviromentContentType.Single;
	        }
			else if (content != null)
	        {
		        throw new ArgumentException("Wrong argument passed to enviroment message constructor.");
	        }

            Message = message;
            MessageType = type;
        }

		public IList<DatabaseEntityItem> MultipleContent { get; }
        public DatabaseEntityItem SingleContent { get; }
		public EnviromentContentType ContentType { get; }
		public string Message { get; }
        public EnviromentMessageType MessageType { get; }
    }

    public enum EnviromentMessageType
    {
        Success,
        Error,
        Warning
    }

	public enum EnviromentContentType
	{
		Single,
		Multiple
	}
}