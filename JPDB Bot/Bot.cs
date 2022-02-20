using System;
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
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using DSharpPlus.VoiceNext;
using JPDB_Bot.Commands;
using JPDB_Bot.StudyLog;

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
                Program.printError("Couldn't read config.json");
                return;
            }


            try
            {
                configJson = JsonConvert.DeserializeObject<ConfigJson>(json);
            }
            catch
            {
                Program.printError("Couldn't deserialize config.json");
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
                Timeout = TimeSpan.FromSeconds(35),
            });
            Client.Ready += Client_Ready;
            Client.MessageCreated += Message_Sent;
            Client.GuildMemberUpdated += Member_Updated;
            Client.TypingStarted += New_Message;
            Client.UseVoiceNext();
            Client.VoiceStateUpdated += voiceStateUpdated;
            //Client.UserJoined +=
            


            //DiscordChannel studyRoom = await Client.GetChannelAsync(929740974568136735);
            //await Client.UseVoiceNext().ConnectAsync(studyRoom).ConfigureAwait(false);

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
                DmHelp = false,
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
            Commands.RegisterCommands<Rules>();
            Commands.RegisterCommands<RoleCommand>();
            Commands.RegisterCommands<JPDB_Search>();
            Commands.RegisterCommands<Study_Command>();
            //Commands.RegisterCommands<TextGame>();
            await Client.ConnectAsync();

            jpdbGuild = await Client.GetGuildAsync(799891866924875786).ConfigureAwait(false);
            await Task.Delay(-1);
        }

        private async Task studyUserJoin(DiscordUser user)
        {
            string txtName = $"{user.Id}_study.txt";
            string txtPath = $"User Data\\{txtName}";

            if (Directory.Exists("User Data") == false) { Directory.CreateDirectory("User Data"); }

            if (File.Exists($"User Data\\{txtName}") == false) { File.Create($"User Data\\{txtName}").Dispose(); }

            await File.AppendAllTextAsync(txtPath, $"j|{DateTime.Now}\n");
        }

        private async Task studyUserLeave(DiscordUser user)
        {
            string txtName = $"{user.Id}_study.txt";
            string txtPath = $"User Data\\{txtName}";

            if (Directory.Exists("User Data") == false) { Directory.CreateDirectory("User Data"); }

            if (File.Exists($"User Data\\{txtName}") == false) { File.Create($"User Data\\{txtName}").Dispose(); }

            await File.AppendAllTextAsync(txtPath, $"l|{DateTime.Now}\n");
        }

        public static async Task studyUserLogRequest(DiscordUser user, CommandContext ctx)
        {
            string txtName = $"{user.Id}_study.txt";
            string txtPath = $"User Data\\{txtName}";

            if (Directory.Exists("User Data") == false) { Directory.CreateDirectory("User Data"); }

            if (Directory.Exists($"User Data\\{txtName}") == false)
            {
                string logText;
                logText = await File.ReadAllTextAsync(txtPath);
                if (logText.Contains("System")) { Program.printError($"Failed to send user {user.Username} their log"); return; }

                DiscordMember usermember;
                try
                {
                    usermember = await ctx.Guild.GetMemberAsync(user.Id).ConfigureAwait(false);
                    await usermember.SendMessageAsync($"```\n{logText}\n```");
                }
                catch { await ctx.RespondAsync("Failed to send you a log").ConfigureAwait(false); }
            }
        }

        public static async Task studyUserLogDelete(DiscordUser user, CommandContext ctx)
        {
            string txtName = $"{user.Id}_study.txt";
            string txtPath = $"User Data\\{txtName}";

            File.Delete(txtPath);
        }

        private async Task voiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs channelChange)
        {
            DiscordChannel quietStudy = await client.GetChannelAsync(929740974568136735).ConfigureAwait(false); //study-hall: 929740974568136735 //test: 799891866924875791
            DiscordChannel afterChannel;
            try { afterChannel = channelChange.After.Channel; } catch { return; }

            DiscordRole giveRole;
            DiscordMember giveMember;
            try
            {
                giveRole = channelChange.Guild.GetRole(929743168923136080); //929743168923136080
                giveMember = await channelChange.Guild.GetMemberAsync(channelChange.User.Id).ConfigureAwait(false);
            }
            catch { return; }

            DiscordChannel beforeChannel= null;
            try { beforeChannel = channelChange.Before.Channel; }
            catch
            {
                if (afterChannel == quietStudy)
                {
                    Program.printMessage($"User {channelChange.User.Username} joined study-hall");
                    await studyUserJoin(channelChange.User);
                    try
                    {
                        foreach (DiscordRole role in giveMember.Roles)
                        {
                            string roleName = role.Name.ToLower();
                            if (roleName == "quiet study")
                            {
                                await giveMember.GrantRoleAsync(giveRole).ConfigureAwait(false);
                            }
                        }
                        return;
                    }
                    catch { return; }
                }
            }
            

            try
            {
                if ((beforeChannel != afterChannel || (beforeChannel == null && afterChannel == null)) && channelChange.Channel == quietStudy)
                {
                    Program.printMessage($"User {channelChange.User.Username} joined study-hall");
                    try
                    {
                        await studyUserJoin(channelChange.User);
                    }
                    catch { }
                    try
                    {
                        foreach (DiscordRole role in giveMember.Roles)
                        {
                            string roleName = role.Name.ToLower();
                            if (roleName == "quiet study")
                            {
                                await giveMember.GrantRoleAsync(giveRole).ConfigureAwait(false);
                            }
                        }
                    }
                    catch { }
                }
                else if (beforeChannel == quietStudy && afterChannel != beforeChannel)
                {
                    try
                    {
                        await studyUserLeave(channelChange.User);
                    } catch { }
                    Program.printMessage($"User {channelChange.User.Username} left study-hall");
                    try
                    {
                        await giveMember.RevokeRoleAsync(giveRole).ConfigureAwait(false);
                    } catch { }
                }
            }
            catch { return; }
        }

        public static DiscordGuild jpdbGuild = null;

        enum roles
        {
            supporter = 0,
            sponsor = 1,
            vip = 2,
            legend = 3,
        }
        private async Task Member_Updated(DiscordClient sender, GuildMemberUpdateEventArgs e)
        {
            await roleUpdateAlert(getHighestRole(e.RolesBefore.ToArray()), getHighestRole(e.RolesAfter.ToArray()), e.Member.Id.ToString(), e);
        }

        private async Task roleUpdateAlert(int highestBefore, int highestAfter, string usernameID, GuildMemberUpdateEventArgs e)
        {
            if (highestAfter <= highestBefore) { return; }

            try
            {
                DiscordMember Kou = await e.Guild.GetMemberAsync(118408957416046593); //kou: 118408957416046593  //jawgboi: 630381088404930560
                await Kou.SendMessageAsync($"<@{usernameID}> is now a {(roles)highestAfter}.");
                Program.printCommandUse($"<@{usernameID}> is now a {(roles)highestAfter}.", "Role update nofication");
            }
            catch (Exception ex)
            {
                Program.printError($"{ex.Message}: Tried to send role-update message but couldn't");
                return;
            }
        }
        private int getHighestRole(DiscordRole[] inputRoles)
        {
            int highestInt = -1;
            foreach (DiscordRole role in inputRoles)
            {
                string roleName = role.Name.ToLower();
                if (roleName != "supporter" && roleName != "sponsor" && roleName != "vip" && roleName != "legend") { continue; }
                roles roleOutput;
                Enum.TryParse(role.Name.ToLower(), out roleOutput);
                int outputInt = (int)roleOutput;
                if (outputInt > highestInt) { highestInt = outputInt; }
            }

            return highestInt;
        }

        private async Task Message_Sent(DiscordClient sender, MessageCreateEventArgs e)
        {   
            if (e.Message.Content.ToLower().Contains("anything in japanese") == true && e.Message.Content.Length < 26)
            {
                await e.Channel.SendMessageAsync("nihongo jouzu");
                return;
            }
            
            if (e.Message.Content.ToLower().Contains("<@!874240645995331585> ") == true)
            {
                if (e.Message.Content.ToLower().Contains("when is the next update") == true
                    || e.Message.Content.ToLower().Contains("when will the next update") == true
                    || e.Message.Content.ToLower().Contains("do you know when the next update") == true
                    || (e.Message.Content.ToLower().Contains("when will kou") == true && (e.Message.Content.ToLower().Contains("update"))))
                {
                    System.Threading.Thread.Sleep(600);
                    await e.Channel.TriggerTypingAsync();
                    System.Threading.Thread.Sleep(2000);

                    string responseString = "Why would I know??";
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                    {
                        responseString = "If all goes to plan it should be today! :O";
                    }
                    else if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
                    {
                        responseString = "Perhaps tomorrow, but who knows? :O";
                    }
                    await e.Channel.SendMessageAsync(responseString).ConfigureAwait(false);
                }
            }


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
                if (userCount == 750) { userCountS = "750"; }
                if (userCount == 869) { userCountS = "869 (nice)"; }
                if (userCount == 1000) { userCountS = "1000"; }
                if (userCount == 1500) { userCountS = "1500"; }
                if (userCount == 2000) { userCountS = "2000"; }
                if (userCount == 2500) { userCountS = "2500"; }
                if (userCount == 3000) { userCountS = "3000"; }
                if (userCount == 3500) { userCountS = "3500"; }
                if (userCount == 4000) { userCountS = "4000"; }
                if (userCount == 4500) { userCountS = "4500"; }
                if (userCount == 5000) { userCountS = "5000"; }
                if (userCount == 6969) { userCountS = "6969"; }
                if (userCount == 10000) { userCountS = "10000"; }
                if (userCountS != string.Empty)
                {
                    try
                    {
                        DiscordMember Kou = await e.Guild.GetMemberAsync(118408957416046593);
                        await Kou.SendMessageAsync($"The server has reached {userCountS} members thanks to {e.Message.Author.Username}");
                        Program.printCommandUse(e.Author.Username, "Member milestone message");
                    }
                    catch (Exception ex)
                    {
                        Program.printError($"{ex.Message}: Tried to send role-update message but couldn't");
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
                    Program.printError("Failed to react to a user join message with :hello");
                }
                try
                {
                    DSharpPlus.Entities.DiscordChannel welcomeChannel = e.Guild.GetChannel(configJson.WelcomeChannelID);
                    //await sender.SendMessageAsync(welcomeChannel, $"Welcome to the official jpdb.io discord server, {e.Author.Username}, a website for learning words and kanji using difficulty lists - prebuilt decks containing all the words in your favourite pieces of content!\nMake sure you read the <#812300824088018945> and if you want to learn more, check the pinned message in <#833939726078967808> for a guide on how to get started with jpdb :)").ConfigureAwait(false);
                    await e.Message.RespondAsync($"Welcome to the official jpdb.io discord server, {e.Author.Username}, a website for learning words and kanji using difficulty lists - prebuilt decks containing all the words in your favourite pieces of content!\nMake sure you read the <#812300824088018945> and if you want to learn more, check the pinned message in <#833939726078967808> for a guide on how to get started with jpdb :)").ConfigureAwait(false);
                } catch (Exception ex)
                {
                    Program.printError(ex.Message + $"\nFailed to welcome user {e.Author.Username}");
                }
                Program.printCommandUse(e.Message.Author.Username, $"Server Join ({userCount})");
                return;
            }

            if (e.Message.Content.ToLower().Contains("boku no pico"))
            {
                await Task.Delay(5000).ConfigureAwait(false);
                await e.Message.CreateReactionAsync(DiscordEmoji.FromName(sender, ":guilty:"));
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

                    Program.printCommandUse(e.Author.Username, "(Content request) " + e.Message.Content);
                    var Kou = await sender.GetUserAsync(118408957416046593);
                    Program.printError("A message would have triggered a content request reply message but was purposefully blocked");
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
        
        private async Task New_Message(DiscordClient sender, TypingStartEventArgs e)
        {
            if (e.Channel.Id == 827482133400256542)
            {
                DiscordMember Kou = await e.Guild.GetMemberAsync(630381088404930560);
                await Kou.SendMessageAsync($"Test <@630381088404930560>");
            }
        }

        private Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            Client.UpdateStatusAsync(new DiscordActivity() { Name = "jpdb.io" }, UserStatus.DoNotDisturb).ConfigureAwait(false);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"JPDB Bot is online in {sender.Guilds.Count()} servers");
            Console.ForegroundColor = ConsoleColor.White;

            return Task.CompletedTask;
        }

    }
}