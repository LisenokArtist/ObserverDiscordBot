using System.Diagnostics;

namespace DiscordBot
{
    public static class Program
    {
        public static DiscordBot DiscordBot { get; private set; }

        static async Task Main(string[] _)
        {
            try
            {
                using (DiscordBot = new DiscordBot())
                {
                    while (!DiscordBot.IsDisposed) { }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}