using DSharpPlus.Entities;
using System;

namespace JPDB_Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Bot bot = null;
            int restarts = 0;
Restart:
            Console.ForegroundColor = ConsoleColor.White;
            if (restarts <= 3)
            {
                try
                {
                    bot = new Bot();
                    bot.RunAsync().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Program.printError($"\nFatal error - {ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
                    restarts += 1;
                    Console.Beep();
                    goto Restart;
                }
            }
            
            Console.Beep();
            Program.printError("Program ended");
            Console.ReadLine();
        }

        public static void printError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void printCommandUse(string user, string command)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"[{DateTime.Now}] {user}: {command}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void printAPIUse(string command, string url)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[{DateTime.Now}] API ({command}): {url}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void printMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[{DateTime.Now}] {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

    }
}
