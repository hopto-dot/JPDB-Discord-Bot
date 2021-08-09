using System;

namespace DiscordBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Bot bot = new Bot();
            try
            {
                bot.RunAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Program.PrintError(ex.ToString());
            }
            bot.
        }

        public static void PrintError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
