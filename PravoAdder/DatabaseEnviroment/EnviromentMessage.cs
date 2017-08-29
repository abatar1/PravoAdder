namespace PravoAdder.DatabaseEnviroment
{
	public class EnviromentMessage
	{
		public dynamic Content { get; }
		public string Message { get; }
		public EnviromentMessageType Type { get; }

		public EnviromentMessage(string content, string message, EnviromentMessageType type)
		{
			Content = content;
			Message = message;
			Type = type;
		}
	}

	public enum EnviromentMessageType
	{
		Success,
		Error,
		Warning
	}
}