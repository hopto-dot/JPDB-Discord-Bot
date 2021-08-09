using Newtonsoft.Json;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordBot.Commands;
using System.Net;

namespace DiscordBot
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public async Task RunAsync()
        {
            //Console.WriteLine("Would you like to enable debug logging (y/n)");
            bool debug = false;
            /*if (Console.ReadLine().Contains("y") == true)
            {
                debug = true;
            }*/
            Console.WriteLine("Reading config file...");
            var json = string.Empty;

            try
            {
                using (var fs = File.OpenRead("config.json"))
                {
                    using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    {
                        json = await sr.ReadToEndAsync().ConfigureAwait(false);

                    }
                }
            } catch
            {
                Program.PrintError("Couldn't read config.json");
            }
            

            ConfigJson configJson;
            try
            {
                configJson = JsonConvert.DeserializeObject<ConfigJson>(json);
            } catch
            {
                Program.PrintError("Couldn't deserialize config.json");
                return;
            }
            
            DiscordConfiguration config;
            if (debug == false)
            {
                config = new DiscordConfiguration
                {
                    Token = configJson.DiscordToken,
                    TokenType = TokenType.Bot,
                    AutoReconnect = true,
                    MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Warning,

                };
            } else
            {
                config = new DiscordConfiguration
                {
                    Token = configJson.DiscordToken,
                    TokenType = TokenType.Bot,
                    AutoReconnect = true,
                    MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
                };
            }

            Console.WriteLine("Connecting bot...");
            Client = new DiscordClient(config);
            Client.Ready += Client_Ready;
            //Client.MessageCreated

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { configJson.Prefix },
                EnableMentionPrefix = true,
                EnableDms = false,
                DmHelp = false,
                EnableDefaultHelp = true,
                IgnoreExtraArguments = true,
            };
            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<TestCommands>();
            await Client.ConnectAsync();
            await Task.Delay(-1);
            
        }
        private Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            Console.WriteLine("JPDB Bot is ready.");
            return Task.CompletedTask;
        }

    }
}
