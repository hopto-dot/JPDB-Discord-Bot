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
using Microsoft.Extensions.DependencyInjection;

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
                Timeout = TimeSpan.FromSeconds(35)
            });
            Client.Ready += Client_Ready;
            Client.MessageCreated += Bot_MessageCreated;

            // Dependency injection for Commands
            ServiceProvider services = new ServiceCollection()
                .AddSingleton<Random>(new Random())
                .AddSingleton<GreetingsDB>(LoadGreetings())
                .BuildServiceProvider();

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { configJson.Prefix },
                EnableMentionPrefix = true,
                EnableDms = false,
                DmHelp = false,
                EnableDefaultHelp = true,
                IgnoreExtraArguments = true,
                Services = services,
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
            if (e.Channel.Name.Contains("meme") && e.Message.Attachments.Count > 0)
            {
                //await e.Message.CreateReactionAsync(DiscordEmoji.FromName(sender, ":thumbsup:"));
                //await e.Message.CreateReactionAsync(DiscordEmoji.FromName(sender, ":thumbsdown:"));
            }

            if (e.Guild.GetMemberAsync(e.Author.Id).Result.Roles.Any(r => r.Name == "Owner" || r.Name == "Supporter" || r.Name == "Server Booster") != true || e.Channel.Name == "bot")
            {
                if ((e.Message.Content.ToLower().Contains("how") && e.Message.Content.ToLower().Contains("do") && e.Message.Content.ToLower().Contains("request")) || (e.Message.Content.ToLower().Contains("request") && e.Message.Content.ToLower().Contains("added")) || (e.Message.Content.ToLower().Contains("novel") && e.Message.Content.ToLower().Contains("request")) || (e.Message.Content.ToLower().Contains("anime") && e.Message.Content.ToLower().Contains("request")) || (e.Message.Content.ToLower().Contains("novel") && e.Message.Content.ToLower().Contains("add")) || (e.Message.Content.ToLower().Contains("anime") && e.Message.Content.ToLower().Contains("add")) || (e.Message.Content.ToLower().Contains("how") && e.Message.Content.ToLower().Contains("add") && e.Message.Content.ToLower().Contains("database") || (e.Message.Content.ToLower().Contains("do") && e.Message.Content.ToLower().Contains("take") && e.Message.Content.ToLower().Contains("requests") || (e.Message.Content.ToLower().Contains("can you add") && e.Message.Content.ToLower().Contains("to") && e.Message.Content.ToLower().Contains("database")) || (e.Message.Content.ToLower().Contains("do") && e.Message.Content.ToLower().Contains("take") && e.Message.Content.ToLower().Contains("requests") || (e.Message.Content.ToLower().Contains("can you add") && e.Message.Content.ToLower().Contains("to") && e.Message.Content.ToLower().Contains("list"))))))
                {
                    if (e.Message.Content.ToLower().Contains("feature") == true)
                    {
                        return;
                    }
                    Program.PrintCommandUse(e.Author.Username, "(Content request) " + e.Message.Content);
                    var Kou = await sender.GetUserAsync(118408957416046593);
                    if (Kou.Presence.Status != DSharpPlus.Entities.UserStatus.Offline)
                    {
                        await e.Message.RespondAsync("To request content you must DM -こう-.\nDo **not** post the script here (see rule 4).").ConfigureAwait(false);
                    }
                    else
                    {
                        await e.Message.RespondAsync("To request content you must DM -こう-. Currently, he's not online.\nDo **not** post the script here (see rule 4).").ConfigureAwait(false);
                    }

                }
            }
            return;
        }

        private Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("JPDB Bot is online.");
            Console.ForegroundColor = ConsoleColor.White;
            return Task.CompletedTask;
        }

        public static GreetingsDB LoadGreetings()
        {
            try
            {
                using FileStream fs = File.OpenRead("greetings.json");
                using StreamReader sr = new(fs, new UTF8Encoding(false));
                string json = sr.ReadToEnd();
                var data = JsonConvert.DeserializeObject<GreetingsDB>(json);
                if (data == null)
                    throw new NullReferenceException();
                return data;
            }
            catch
            {
                WeightedString[] supporterGreetings =
                {
                    new WeightedString { Value = "Hi %username%様, I can speak English too ya know >:)", Weight = 1 },
                    new WeightedString { Value = "どうも、%username%様 :)", Weight = 5 },
                    new WeightedString { Value = "よおおおおおおおぉ %username%様！ :)", Weight = 1 },
                    new WeightedString { Value = "また会えて嬉しいね %username%様 :)", Weight = 3 },
                    new WeightedString { Value = "やっほおおおおおお～ %username%様 :)", Weight = 1 },
                    new WeightedString { Value = "おおおおっす! %username%様 :)", Weight = 1 },
                    new WeightedString { Value = "ハロオオオ %username%様！ :)", Weight = 1 },
                    new WeightedString { Value = "へっ！なんかあった？%username%様 :O", Weight = 2 },
                };

                WeightedString[] defaultGreetings =
                {
                    new WeightedString { Value = "どうも、%username%様 :)", Weight = 5 },
                    new WeightedString { Value = "おいお前 JPDBの支援者になれ", Weight = 1 },
                    new WeightedString { Value = "元気はないんだなあ %username%さん。JPDBを支援したら？うwう", Weight = 5 },
                    new WeightedString { Value = "おっす! %username% :)", Weight = 1 },
                    new WeightedString { Value = "ハロー %username%！ :)", Weight = 2 },
                    new WeightedString { Value = "へっ！なんかあった？%username%様 :O", Weight = 1 },
                };

                GreetingsDB data = new GreetingsDB
                {
                    { "Supporter", supporterGreetings },
                    { "Default", defaultGreetings }
                };

                return data;
            }
        }
    }
}
