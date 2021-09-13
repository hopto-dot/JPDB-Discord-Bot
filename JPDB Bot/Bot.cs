using Newtonsoft.Json;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordBot.Commands;
using System.Net;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;

namespace DiscordBot
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public async Task RunAsync()
        {
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
                return;
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
            
            Microsoft.Extensions.Logging.LogLevel LoggingLevel = Microsoft.Extensions.Logging.LogLevel.Warning;
            if (configJson.LogLevel.ToLower() == "debug")
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Debug logging enabled.");
                Console.ForegroundColor = ConsoleColor.White;
                LoggingLevel = Microsoft.Extensions.Logging.LogLevel.Debug;
            }
            DiscordConfiguration config;
            
            config = new DiscordConfiguration
            {
                Token = configJson.DiscordToken,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LoggingLevel,
                Intents = DiscordIntents.All,
            };

            Console.WriteLine("Connecting bot...");
            Client = new DiscordClient(config);
            Client.UseInteractivity(new InteractivityConfiguration()
            {
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });
            Client.Ready += Client_Ready;
            Client.MessageCreated += Bot_MessageCreated;

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

        private async Task Bot_MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            //await e.Message.RespondAsync("test").ConfigureAwait(false);
            //await e.Channel.SendMessageAsync("test").ConfigureAwait(false);
            if (e.Guild.Name != "Test Server" || e.Author.IsBot)
            {
                if (e.Channel.Name.Contains("meme") && e.Message.Attachments.Count > 0)
                {
                    //await e.Message.CreateReactionAsync(DiscordEmoji.FromName(sender, ":thumbsup:"));
                    //await e.Message.CreateReactionAsync(DiscordEmoji.FromName(sender, ":thumbsdown:"));
                }

                if (e.Guild.GetMemberAsync(e.Author.Id).Result.Roles.Any(r => r.Name == "Owner" || r.Name == "Supporter" || r.Name == "Server Booster") != true || e.Channel.Name == "bot")
                {
                    if ((e.Message.Content.Contains("how") && e.Message.Content.Contains("do") && e.Message.Content.Contains("request")) || (e.Message.Content.Contains("request") && e.Message.Content.Contains("added")) || (e.Message.Content.Contains("novel") && e.Message.Content.Contains("request")) || (e.Message.Content.Contains("anime") && e.Message.Content.Contains("request")) || (e.Message.Content.Contains("novel") && e.Message.Content.Contains("add")) || (e.Message.Content.Contains("anime") && e.Message.Content.Contains("add")) || (e.Message.Content.Contains("how") && e.Message.Content.Contains("add") && e.Message.Content.Contains("database") || (e.Message.Content.Contains("do") && e.Message.Content.Contains("take") && e.Message.Content.Contains("requests") || (e.Message.Content.Contains("can you add") && e.Message.Content.Contains("to") && e.Message.Content.Contains("database")) || (e.Message.Content.Contains("do") && e.Message.Content.Contains("take") && e.Message.Content.Contains("requests") || (e.Message.Content.Contains("can you add") && e.Message.Content.Contains("to") && e.Message.Content.Contains("list"))))))
                    {
                        Program.PrintCommandUse(e.Author.Username, "(Content request) " + e.Message.Content);
                        var Kou = await sender.GetUserAsync(118408957416046593);
                        if (Kou.Presence.Status != DSharpPlus.Entities.UserStatus.Offline)
                        {
                            await e.Message.RespondAsync("To request content you must DM -こう-.\nDo **not** post the script for it here (see rule 4).").ConfigureAwait(false);
                        }
                        else
                        {
                            await e.Message.RespondAsync("To request content you must DM -こう-. Currently, he's not online.\nDo **not** post the script for it here (see rule 4).").ConfigureAwait(false);
                        }

                    }
                }
                return;
            }

            //.Any(r => r.Name == "Owner" || r.Name == "Supporter" || r.Name == "Server Booster") = !true
            

            return;
        }

        private Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("JPDB Bot is online.");
            Console.ForegroundColor = ConsoleColor.White;
            return Task.CompletedTask;
        }
    }
}
