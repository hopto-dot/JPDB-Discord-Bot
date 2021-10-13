﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using JPDB_Bot.Commands;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace JPDB_Bot
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }

        ConfigJson configJson;
        public async Task RunAsync()
        {
            Console.WriteLine("Reading config file...");
            string json;

            try
            {
                using (var fs = File.OpenRead("config.json"))
                {
                    using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    {
                        json = await sr.ReadToEndAsync().ConfigureAwait(false);
                    }
                }
            }
            catch
            {
                Program.PrintError("Couldn't read config.json");
                return;
            }


            try
            {
                configJson = JsonConvert.DeserializeObject<ConfigJson>(json);
            }
            catch
            {
                Program.PrintError("Couldn't deserialize config.json");
                return;
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Settings:");
            Console.WriteLine($"DiscordToken: Working");
            Console.WriteLine($"Prefix: {configJson.Prefix}");
            Console.WriteLine($"JPDBToken: Working");
            Console.WriteLine($"LogLevel: {configJson.LogLevel}");
            Console.WriteLine($"WelcomeMessages: {configJson.WelcomeMessages}");
            Console.WriteLine($"WelcomeChannelID: {configJson.WelcomeChannelID}");
            Console.ForegroundColor = ConsoleColor.White;

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
            Client.GuildMemberUpdated += Member_Updated;

            // Dependency injection for Commands
            ServiceProvider services = new ServiceCollection()
                .AddSingleton<ConfigJson>(configJson)
                .AddSingleton<Random>(new Random())
                .AddSingleton<GreetingsData>(GreetingsData.LoadGreetings())
                .BuildServiceProvider();

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { configJson.Prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                DmHelp = true,
                EnableDefaultHelp = true,
                IgnoreExtraArguments = true,
                Services = services,
            };
            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<Greeter>();
            Commands.RegisterCommands<FreqGameCommand>();
            Commands.RegisterCommands<ChangeLog>();
            Commands.RegisterCommands<Content>();
            Commands.RegisterCommands<JapanTime>();
            Commands.RegisterCommands<MemberCount>();
            await Client.ConnectAsync();

            await Task.Delay(-1);
        }

        private async Task Member_Updated(DiscordClient sender, GuildMemberUpdateEventArgs e)
        {
            DiscordRole[] rolesBefore = e.RolesBefore.ToArray();
            bool supporterB = false;
            bool sponsorB = false;
            bool vipB = false;
            bool legendB = false;
            foreach (DiscordRole roleB in rolesBefore)
            {
                switch (roleB.Name.ToLower())
                {
                    case "supporter":
                        supporterB = true;
                        break;
                    case "sponsor":
                        sponsorB = true;
                        break;
                    case "vip":
                        vipB = true;
                        break;
                    case "legend":

                        break;
                }
            }

            DiscordRole[] rolesAfter = e.RolesAfter.ToArray();
            bool supporterA = false;
            bool sponsorA = false;
            bool vipA = false;
            bool legendA = false;
            foreach (DiscordRole roleA in rolesAfter)
            {
                switch (roleA.Name.ToLower())
                {
                    case "supporter":
                        supporterA = true;
                        break;
                    case "sponsor":
                        sponsorA = true;
                        break;
                    case "vip":
                        vipA = true;
                        break;
                    case "legend":
                        legendA = true;
                        break;
                }
            }
            string patreonChange = "none";
            if (legendA == false && legendB == true) { patreonChange = "legend"; }
            if (vipB == false && vipA == true) { patreonChange = "vip"; }
            if (sponsorB == false && sponsorA == true) { patreonChange = "sponsor"; }
            if (supporterB == false && supporterA == true) { patreonChange = "supporter"; }
            //Program.PrintError($"legend: {legendB} -> {legendA}\n" +
            //    $"vip: {vipB} -> {vipA}\n" +
            //    $"sponsor: {sponsorB} -> {sponsorA}\n" +
            //    $"supporter: {supporterB} -> {supporterA}\n");
            if (patreonChange == "none") { return; }

            try
            {
                DiscordMember Kou = await e.Guild.GetMemberAsync(118408957416046593);
                await Kou.SendMessageAsync($"<@{e.Member.Id}> is now a {patreonChange}.");
                Program.PrintCommandUse(e.Member.Username, "Updated role");
            } catch (Exception ex)
            {
                Program.PrintError($"{ex.Message}: Tried to send role-update message but couldn't");
                return;
            }
        }

        private async Task Bot_MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            //sending messages and replying templates:\
            //await e.Message.RespondAsync("test").ConfigureAwait(false);
            //await e.Channel.SendMessageAsync("test").ConfigureAwait(false);

            try
            {
                if (e.Channel.Name.Contains("meme") && e.Message.Attachments.Count > 0 && configJson.MemeRatings.ToLower() == "enabled")
                {
                    //if in the #memes channel and there is an attachment, reacting with thumbsup and thumbsdown
                    await e.Message.CreateReactionAsync(DiscordEmoji.FromName(sender, ":thumbsup:"));
                    await e.Message.CreateReactionAsync(DiscordEmoji.FromName(sender, ":thumbsdown:"));
                }
            } catch
            {
                return;
            }
            

            if(e.Message.MessageType == MessageType.GuildMemberJoin && (configJson.WelcomeMessages.ToLower() == "enabled" || configJson.WelcomeMessages.ToLower() == "True"))
            {
                int userCount = e.Guild.GetAllMembersAsync().Result.ToArray().Length;
                string userCountS = string.Empty;
                if (userCount == 1000) { userCountS = "1000"; }
                if (userCount == 2000) { userCountS = "2000"; }
                if (userCount == 3000) { userCountS = "3000"; }
                if (userCount == 4000) { userCountS = "4000"; }
                if (userCount == 5000) { userCountS = "5000"; }
                if (userCount == 10000) { userCountS = "10000"; }
                if (userCountS != string.Empty)
                {
                    try
                    {
                        DiscordMember Kou = await e.Guild.GetMemberAsync(118408957416046593);
                        await Kou.SendMessageAsync($"The server has now reached {userCountS} members!");
                        Program.PrintCommandUse(e.Author.Username, "Member milestone message");
                    }
                    catch (Exception ex)
                    {
                        Program.PrintError($"{ex.Message}: Tried to send role-update message but couldn't");
                        return;
                    }
                }


                //when a user joins:
                try
                {
                    string emoji = ":hello:";
                    if (e.Author.Username.ToLower().Contains("boyin"))
                    {
                        emoji = ":eyes:";
                    }
                    await e.Message.CreateReactionAsync(DiscordEmoji.FromName(sender, emoji));
                } catch
                {
                    Program.PrintError("Failed to react to a user join message with :hello");
                }
                try
                {
                    DSharpPlus.Entities.DiscordChannel welcomeChannel = e.Guild.GetChannel(configJson.WelcomeChannelID);
                    //await sender.SendMessageAsync(welcomeChannel, $"Welcome to the official jpdb.io discord server, {e.Author.Username}, a website for learning words and kanji using difficulty lists - prebuilt decks containing all the words in your favourite pieces of content!\nMake sure you read the <#812300824088018945> and if you want to learn more, check the pinned message in <#833939726078967808> for a guide on how to get started with jpdb :)").ConfigureAwait(false);
                    await e.Message.RespondAsync($"Welcome to the official jpdb.io discord server, {e.Author.Username}, a website for learning words and kanji using difficulty lists - prebuilt decks containing all the words in your favourite pieces of content!\nMake sure you read the <#812300824088018945> and if you want to learn more, check the pinned message in <#833939726078967808> for a guide on how to get started with jpdb :)").ConfigureAwait(false);
                } catch (Exception ex)
                {
                    Program.PrintError(ex.Message + $"\nFailed to welcome user {e.Author.Username}");
                }
                Program.PrintCommandUse(e.Message.Author.Username, $"Server Join ({userCount})");
                return;
            }

            if (e.Message.Content.ToLower().Contains("boku no pico"))
            {
                await Task.Delay(5000).ConfigureAwait(false);
                await e.Message.CreateReactionAsync(DiscordEmoji.FromName(sender, ":eyes:"));
                return;
            }

            if (e.Guild.GetMemberAsync(e.Author.Id).Result.Roles.Any(r => r.Name == "Owner" || r.Name == "Supporter" || r.Name == "Server Booster") != true || e.Channel.Name == "bot")
            {
                if ((
                    e.Message.Content.ToLower().Contains("how") && e.Message.Content.ToLower().Contains("do") && e.Message.Content.ToLower().Contains("request")) && e.Message.Content.ToLower().Contains("add") ||
                    (e.Message.Content.ToLower().Contains("how") && e.Message.Content.ToLower().Contains("request") && e.Message.Content.ToLower().Contains("added"))
                    || (e.Message.Content.ToLower().Contains("i want") && e.Message.Content.ToLower().Contains("add") && e.Message.Content.ToLower().Contains("database"))
                    || (e.Message.Content.ToLower().Contains("this novel") && e.Message.Content.ToLower().Contains("add") && e.Message.Content.ToLower().Contains("to the"))
                    || (e.Message.Content.ToLower().Contains("anime") && e.Message.Content.ToLower().Contains("add") && e.Message.Content.ToLower().Contains("to the"))
                    || (e.Message.Content.ToLower().Contains("how") && e.Message.Content.ToLower().Contains("add") && e.Message.Content.ToLower().Contains("to the") && e.Message.Content.ToLower().Contains("database") 
                    || (e.Message.Content.ToLower().Contains("do") && e.Message.Content.ToLower().Contains("take") && e.Message.Content.ToLower().Contains("request") 
                    || (e.Message.Content.ToLower().Contains("can you add") && e.Message.Content.ToLower().Contains("to") && e.Message.Content.ToLower().Contains("database")) 
                    || (e.Message.Content.ToLower().Contains("do") && e.Message.Content.ToLower().Contains("take") && e.Message.Content.ToLower().Contains("request")
                    || (e.Message.Content.ToLower().Contains("please") && e.Message.Content.ToLower().Contains("add") && e.Message.Content.ToLower().Contains("to the "))
                    || (e.Message.Content.ToLower().Contains("please") && e.Message.Content.ToLower().Contains("add") && e.Message.Content.ToLower().Contains("to the "))
                    || (e.Message.Content.ToLower().Contains("where ") && e.Message.Content.ToLower().Contains("ask") && e.Message.Content.ToLower().Contains("add"))
                    || (e.Message.Content.ToLower().Contains("where can i request"))
                    || (e.Message.Content.ToLower().Contains("where do you request"))
                    || (e.Message.Content.ToLower().Contains("can you add") && e.Message.Content.ToLower().Contains("to") && e.Message.Content.ToLower().Contains("list"))))))
                {
                    if (e.Message.Content.ToLower().Contains("feature") == true || e.Message.Content.ToLower().Contains("idea"))
                    {
                        return;
                    }

                    Program.PrintCommandUse(e.Author.Username, "(Content request) " + e.Message.Content);
                    var Kou = await sender.GetUserAsync(118408957416046593);
                    Program.PrintError("A message would have triggered a content request reply message but was purposefully blocked");
                    if (Kou.Presence.Status != DSharpPlus.Entities.UserStatus.Offline)
                    {
                        //await e.Message.RespondAsync("To request content you must DM -こう-.\nDo **not** post the script here (see rule 4).").ConfigureAwait(false);
                    }
                    else
                    {
                        //await e.Message.RespondAsync("To request content you must DM -こう-. Currently, he's not online.\nDo **not** post the script here (see rule 4).").ConfigureAwait(false);
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
    }
}