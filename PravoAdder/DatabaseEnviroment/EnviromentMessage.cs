namespace PravoAdder.DatabaseEnviroment
{
    public class EnviromentMessage
    {
        public string Content { get; }
        public string Message { get; }

        public EnviromentMessage(string content, string message)
        {
            Content = content;
            Message = message;
        }
    }
}
