namespace PravoAdder.DatabaseEnviroment
{
    public class EnviromentMessage
    {
        public EnviromentMessage(string content, string message, EnviromentMessageType type)
        {
            Content = content;
            Message = message;
            Type = type;
        }

        public dynamic Content { get; }
        public string Message { get; }
        public EnviromentMessageType Type { get; }
    }

    public enum EnviromentMessageType
    {
        Success,
        Error,
        Warning
    }
}