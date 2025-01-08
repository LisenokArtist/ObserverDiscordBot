using System.Text;

namespace DiscordBot.Core
{
    public class BotCommandExecuteException : Exception
    {
        public List<string> Errors { get; private set; } = new List<string>();
        
        public BotCommandExecuteException() { }
        
        public BotCommandExecuteException(string error)
        {
            Errors.Add(error);
        }

        public BotCommandExecuteException(string[] errors)
        {
            Errors.AddRange(errors);
        }

        public override string ToString()
        {
            return Errors.ToString() ?? string.Empty;
        }
    }
}
