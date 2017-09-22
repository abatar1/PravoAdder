using PravoAdder.Api.Domain;

namespace PravoAdder.Api
{
    public class EnviromentMessage
    {
        public EnviromentMessage(DatabaseEntityItem content, string message, EnviromentMessageType type)
        {
            Content = content;
            Message = message;
            Type = type;
        }

        public DatabaseEntityItem Content { get; }
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