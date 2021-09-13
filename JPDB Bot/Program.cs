using System;

namespace DiscordBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Bot bot = new Bot();
            bot.RunAsync().GetAwaiter().GetResult();
            
        }

        public static void PrintError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void PrintCommandUse(string user, string command)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"[{DateTime.Now}] {user}: {command}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void PrintAPIUse(string command, string url)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[{DateTime.Now}] API ({command}): {url}");
            Console.ForegroundColor = ConsoleColor.White;
        }

    }
}
