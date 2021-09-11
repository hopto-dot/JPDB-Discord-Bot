using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Entities;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiscordBot.Commands
{
    public class TestCommands : BaseCommandModule
    {
        [Command("hi")]
        [Cooldown(2, 10, CooldownBucketType.User)]
        [Description("Get a nice (or bad) response")]
        public async Task hi(CommandContext ctx)
        {
            Program.PrintCommandUse(ctx.User.Username, ctx.Message.Content);
            Random random = new Random();
            int randomInt = random.Next(1, 16);
            string Output = string.Empty;
            if (ctx.Member.Roles.Any(r => r.Name == "Owner" || r.Name == "Supporter" || r.Name == "Server Booster"))
            {
                switch (randomInt)
                {
                    case 1:
                        goto case 7;
                    case 2:
                        goto case 7;
                    case 3:
                        goto case 7;
                    case 4:
                        goto case 15;
                    case 5:
                        Output = "Hi " + ctx.User.Username + "様, I can speak English too ya know >:)";
                        break;
                    case 6:
                        goto case 11;
                    case 7:
                        Output = "どうも、" + ctx.User.Username + "様 :)";
                        break;
                    case 8:
                        Output = "どうも、" + ctx.User.Username + "様 ;)";
                        break;
                    case 9:
                        Output = "よおおおおおおおぉ " + ctx.User.Username + "様！ :)";
                        break;
                    case 10:
                        goto case 11;
                    case 11:
                        Output = "また会えて嬉しいね " + ctx.User.Username + "様 :)";
                        break;
                    case 12:
                        Output = "やっほおおおおおお～ " + ctx.User.Username + "様 :)";
                        break;
                    case 13:
                        Output = "おおおおっす! " + ctx.User.Username + "様 :)";
                        break;
                    case 14:
                        Output = "ハロオオオ " + ctx.User.Username + "様！ :)";
                        break;
                    case 15:
                        Output = "へっ！なんかあった？" + ctx.User.Username + "様 :O";
                        break;
                }
            }
            else
            {
                switch (randomInt)
                {
                    case 1:
                        goto case 7;
                    case 2:
                        goto case 7;
                    case 3:
                        goto case 7;
                    case 4:
                        goto case 11;
                    case 5:
                        goto case 12;
                    case 6:
                        goto case 14;
                    case 7:
                        Output = "どうも、" + ctx.User.Username + " :)";
                        break;
                    case 8:
                        Output = "どうも、" + ctx.User.Username + " ;)";
                        break;
                    case 9:
                        Output = "おいお前 JPDBの支援者になれ";
                        break;
                    case 10:
                        goto case 12;
                    case 11:
                        goto case 12;
                    case 12:
                        Output = "元気はないんだなあ " + ctx.User.Username + "さん。JPDBを支援したら？うwう";
                        break;
                    case 13:
                        Output = "おっす! " + ctx.User.Username + " :)";
                        break;
                    case 14:
                        Output = "ハロー " + ctx.User.Username + "！ :)";
                        break;
                    case 15:
                        Output = "へっ！なんかあった？" + ctx.User.Username + " :O";
                        break;
                }
            }
            await ctx.RespondAsync(Output).ConfigureAwait(false);
        }

        [Command("freqgame")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        [Description("Play a game where you guess which word is more frequent")]
        public async Task guessgame(CommandContext ctx, [DescriptionAttribute("Your jpdb username")] string jpdbUser = "", [DescriptionAttribute("Number of rounds")] int rounds = 5)
        {
            if (ctx.Guild.Name == "jpdb.io official")
            {
                //await ctx.Channel.SendMessageAsync($"The bot is currently being tested");
                //return;
            }

            Program.PrintCommandUse(ctx.User.Username, ctx.Message.Content);

            double answerTime = 4;

            DiscordUser Player1 = ctx.Message.Author;

            await ctx.Channel.SendMessageAsync($"Type \"!me [jpdb username]\" to play with {ctx.User.Username}, a jpdb username isn't required.\nType \"!start\" once you're all ready.");
            bool gameReady = false;
            List<gamePlayer> players = new List<gamePlayer>();

            gamePlayer newPlayer = new gamePlayer()
            {
                username = ctx.Message.Author.Username,
                jpdbUsername = ctx.Message.Content.ToLower().Length > 9 ? ctx.Message.Content.ToLower().Substring(10) : string.Empty,
            };
            players.Add(newPlayer);
            DSharpPlus.Interactivity.InteractivityResult<DSharpPlus.Entities.DiscordMessage> result;
            try
            {
                do
                {
                    result = await ctx.Channel.GetNextMessageAsync(m =>
                    {
                        foreach (gamePlayer player in players)
                        {
                            if (player.username == m.Author.Username && m.Content != "!start")
                            {
                                return false;
                            }
                            if (m.Content.Length > 4)
                            {
                                if (m.Content.ToLower().Substring(0, 4) == "!me ")
                                {
                                    if (m.Content.ToLower().Substring(4) == player.jpdbUsername)
                                    {
                                        return false; //if username in "!me [username]" is the same as one of the existing game players
                                    }
                                }
                            }
                        };

                        if (m.Content == "!start")
                        {
                            gameReady = true;
                            return m.Content.ToLower() == "!start";
                        }
                        else if (m.Content.ToLower().Length == 3)
                        {
                            gamePlayer newPlayer = new gamePlayer()
                            {
                                username = m.Author.Username
                            };
                            players.Add(newPlayer);
                            return m.Content.ToLower() == "!me";
                        }
                        else if (m.Content.ToLower().Length > 4)
                        {
                            if (m.Content.ToLower().Substring(0, 4) == "!me ")
                            {
                                gamePlayer newPlayer = new gamePlayer()
                                {
                                    username = m.Author.Username,
                                    jpdbUsername = m.Content.ToLower().Substring(4),
                                };

                                players.Add(newPlayer);
                                return m.Content.ToLower().Substring(5) == "!me ";
                            }
                            else
                            {
                                return false;
                            }

                        }
                        else
                        {
                            gameReady = false;
                            return false;
                        }
                    }).ConfigureAwait(false);

                    if (result.TimedOut)
                    {
                        await ctx.Channel.SendMessageAsync("Game timed out").ConfigureAwait(false);
                        return;
                    }
                    foreach (gamePlayer Person in players)
                    {
                        if (result.Result.Content.Contains(Person.username) && result.Result.Content != "!start")
                        {
                            await ctx.RespondAsync("You can't play against yourself lol").ConfigureAwait(false);
                            gameReady = false;
                        }
                    }

                } while (gameReady == false);
            }
            catch
            {
                await ctx.Channel.SendMessageAsync("Game timed out.").ConfigureAwait(false);
                return;
            }

            //await ctx.Channel.SendMessageAsync(User2);
            List<string> playerNames = new List<string>();
            foreach (gamePlayer player in players)
            {
                string playerName = player.jpdbUsername;
                if (playerName == "")
                {
                    playerName = "*No jpdb name*";
                }
                playerNames.Add(player.username + $" ({playerName})");
            }

            await ctx.Channel.SendMessageAsync($"{string.Join(" 対 ", playerNames)}").ConfigureAwait(false);

            var gameEmbed = new DiscordEmbedBuilder
            {
                Title = $"Freq guessing game",
                Description = $"**Guess which word is more frequent**\n\nParticipants:\n" + string.Join("\n", playerNames),
                Color = DiscordColor.Red,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "Currently, usernames don't do anything.",
                }
            };
            await ctx.Channel.SendMessageAsync(embed: gameEmbed).ConfigureAwait(false);

            await Task.Delay(1500);

            Console.ForegroundColor = ConsoleColor.Green;
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
            }
            catch
            {
                Program.PrintError("Couldn't read config.json");
                return;
            }

            ConfigJson configJson;
            try
            {
                configJson = JsonConvert.DeserializeObject<ConfigJson>(json);
            }
            catch
            {
                Program.PrintError("Couldn't deserialize config.json");
                return;
            }

            Random random = new Random();
            
            if (rounds < 1 || rounds > 20)
            {
                rounds = 5;
            }
            for (int round = 1; round <= rounds; round++)
            {
                


                WebRequest request; ///pick_words?count=2&spread=100&users=user1,user2,user3
                //request = WebRequest.Create("https://jpdb.io/api/experimental/pick_word_pair?rank_at_least=2000&rank_at_most=100&user_1=spectaku&user_2=alemax");
                request = WebRequest.Create("https://jpdb.io/api/experimental/pick_words?count=2&spread=350");//&users=user1,user2,user3";
                //request.Credentials = CredentialCache.DefaultCredentials
                request.Method = "GET";
                request.Headers["Authorization"] = "Bearer " + configJson.JPDBToken;

                HttpWebResponse response = null;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch
                {
                    await ctx.Channel.SendMessageAsync("api request failed").ConfigureAwait(false);
                    return;
                }

                //Console.WriteLine(response.StatusDescription);

                //Get the stream containing content returned by the server.
                Stream dataStream = response.GetResponseStream();
                //Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                //Read the content.
                String responseFromServer = reader.ReadToEnd();
                //Display the content.
                Console.WriteLine(responseFromServer);
                //Clean up the streams and the response.
                reader.Close();
                response.Close();
                responseFromServer = responseFromServer.Replace(@"\" + "\"word" + @"\" + "\"", ""); //  \"words\":
                JToken[] fullJson = JObject.Parse(responseFromServer).SelectToken("words").ToArray();

                Console.WriteLine("Parsed JSON response");

                await Task.Delay(1000);

                Newtonsoft.Json.Linq.JToken token1 = fullJson[0];
                Newtonsoft.Json.Linq.JToken token2 = fullJson[1];

                Vocabulary wordA = new Vocabulary
                {
                    vocabKanji = string.Empty,
                    vocabReading = string.Empty,
                    vocabFreq = -1,
                };
                Vocabulary wordB = new Vocabulary
                {
                    vocabKanji = string.Empty,
                    vocabReading = string.Empty,
                    vocabFreq = -1,
                };

                int randomInt = random.Next(0, 2);
                if (randomInt == 0)
                {
                    wordA = new Vocabulary
                    {
                        vocabKanji = token1.SelectToken("spelling").ToString(),
                        vocabReading = token1.SelectToken("reading").ToString(),
                        vocabFreq = token1.SelectToken("vrank").ToObject<int>(),
                    };
                    wordB = new Vocabulary
                    {
                        vocabKanji = token2.SelectToken("spelling").ToString(),
                        vocabReading = token2.SelectToken("reading").ToString(),
                        vocabFreq = token2.SelectToken("vrank").ToObject<int>(),
                    };
                } else
                {
                    wordA = new Vocabulary
                    {
                        vocabKanji = token2.SelectToken("spelling").ToString(),
                        vocabReading = token2.SelectToken("reading").ToString(),
                        vocabFreq = token2.SelectToken("vrank").ToObject<int>(),
                    };
                    wordB = new Vocabulary
                    {
                        vocabKanji = token1.SelectToken("spelling").ToString(),
                        vocabReading = token1.SelectToken("reading").ToString(),
                        vocabFreq = token1.SelectToken("vrank").ToObject<int>(),
                    };
                }
                //
                


                Console.WriteLine("Parsed words.");


                ////////////////////////END OF API////////////////////////

                //await ctx.Channel.SendMessageAsync(User2);
                List<string> playerPoints = new List<string>();
                foreach (gamePlayer player in players)
                {
                    string playerName = player.jpdbUsername;
                    if (playerName == "")
                    {
                        playerName = "*No jpdb name*";
                    }
                    playerNames.Add($"{player.username}: {player.points})");
                }

                gameEmbed = new DiscordEmbedBuilder
                {
                    Title = "Which word is more frequent?",
                    Description = $"A = {wordA.vocabKanji} ({wordA.vocabReading})\nB = {wordB.vocabKanji} ({wordB.vocabReading})",
                    Color = DiscordColor.Red,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = string.Join("\n", playerPoints)
                    }
                };

                var interactivity = ctx.Client.GetInteractivity();
                var questionMessage = await ctx.Channel.SendMessageAsync(embed: gameEmbed).ConfigureAwait(false);

                await questionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":regional_indicator_a:"));
                await questionMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":regional_indicator_b:"));

                var reactionResult = await interactivity.CollectReactionsAsync(questionMessage, TimeSpan.FromSeconds(answerTime));
                var distinctResult = reactionResult.ToArray();

                //resetting the players' choices
                foreach (gamePlayer Person in players)
                {
                    Person.choice = "0";
                }

                foreach (var Reaction in distinctResult.ToArray())
                {
                    if (Reaction.Emoji.Name == "🇦")
                    {
                        foreach (var User in Reaction.Users.ToArray()) //loop through all the users who've reacted with A
                        {
                            if (User.Username == "JPDB Bot (Unofficial)")
                            {
                                goto SkipA;
                            }
                            foreach (gamePlayer Person in players)
                            {
                                if (Person.username == User.Username)
                                {
                                    if (Person.choice != "0") //if player2 has voted on both options
                                    {
                                        Person.choice = "-1";
                                    }
                                    else
                                    {
                                        Person.choice = "A";
                                    }
                                }
                            }
                        SkipA:;
                        }
                    }
                    else if (Reaction.Emoji.Name == "🇧") //loop through all the users who've reacted with A
                    {
                        foreach (var User in Reaction.Users.ToArray()) //loop through all the users who've reacted with A
                        {
                            if (User.Username == "JPDB Bot (Unofficial)")
                            {
                                goto SkipB;
                            }
                            foreach (gamePlayer Person in players)
                            {
                                if (Person.username == User.Username)
                                {
                                    if (Person.choice != "0") //if player2 has voted on both options
                                    {
                                        Person.choice = "-1";
                                    }
                                    else
                                    {
                                        Person.choice = "B";
                                    }
                                }
                            }
                        SkipB:;
                        }
                    }
                }


                List<string> correctPlayers = new List<string>();
                if (wordA.vocabFreq < wordB.vocabFreq) //if word A is more frequent
                {
                    await ctx.Channel.SendMessageAsync("A was the correct answer!").ConfigureAwait(false);
                    foreach (gamePlayer Person in players)
                    {
                        if (Person.choice == "A")
                        {
                            Person.points += 1;
                            correctPlayers.Add(Person.username);
                        }
                    }
                }
                else //if word B is more frequent
                {
                    await ctx.Channel.SendMessageAsync("B was the correct answer!").ConfigureAwait(false);
                    foreach (gamePlayer Person in players)
                    {
                        if (Person.choice == "B")
                        {
                            Person.points += 1;
                            correctPlayers.Add(Person.username);
                        }
                    }
                }

                if (correctPlayers.Count > 0)
                {
                    await ctx.Channel.SendMessageAsync($"{string.Join(", ", correctPlayers)} got it right!").ConfigureAwait(false);
                }
                else if (correctPlayers.Count == players.Count && players.Count > 1)
                {
                    await ctx.Channel.SendMessageAsync("Everyone got it right!").ConfigureAwait(false);
                }
                else if (correctPlayers.Count == 0 && players.Count > 3)
                {
                    await ctx.Channel.SendMessageAsync("How did no one get that right!?").ConfigureAwait(false);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync("No one got it right!").ConfigureAwait(false);
                }


                if (round != 5)
                {
                    await Task.Delay(2500);
                }
            }

            List<string> endPlayerPoints = new List<string>();
            foreach (gamePlayer player in players)
            {
                endPlayerPoints.Add($"{player.username}: {player.points}");
            }

            gameEmbed = new DiscordEmbedBuilder
            {
                Title = "Game result:",
                Description = $"**Points:**",
                Color = DiscordColor.Red,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = string.Join("\n", endPlayerPoints)
                }
            };
            await ctx.Channel.SendMessageAsync(embed: gameEmbed).ConfigureAwait(false);
        }

        [Command("changelog")]
        [Cooldown(1, 20, CooldownBucketType.User)]
        [Description("Shows the latest addition to the changelog")]
        public async Task changeLog(CommandContext ctx)
        {
            Program.PrintCommandUse(ctx.User.Username, ctx.Message.Content);
            string Date = String.Empty;

            /*WebClient Client = new WebClient();
            Client.Encoding = System.Text.Encoding.UTF8;
            string HTML = "";
            string sniptemp = string.Empty;
            HTML = Client.DownloadString(new Uri("https://jpdb.io/changelog"));
            int snipIndex = HTML.IndexOf("<h5 id=") + 8;
            HTML = HTML.Substring(snipIndex);
            snipIndex = HTML.IndexOf(">") + 1;
            HTML = HTML.Substring(snipIndex);

            sniptemp = HTML;
            snipIndex = sniptemp.IndexOf("<");
            sniptemp = sniptemp.Substring(0, snipIndex);

            var changelogEmbed = new discordembed*/

            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load("https://jpdb.io/changelog");
            string ChangeDate = htmlDoc.DocumentNode.SelectNodes("/html/body/div[2]/h5[1]")
    .First()
    .InnerHtml;
            string Information = htmlDoc.DocumentNode.SelectNodes("/html/body/div[2]/h5[1]").First().NextSibling.InnerHtml;

            await ctx.RespondAsync(ChangeDate).ConfigureAwait(false);
        }

        [Command("content")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [Description("Search for content in the JPDB database\nFor example: !content \"steins gate\"")]
        public async Task content(CommandContext ctx, [DescriptionAttribute("Name of the content you are searching")] string searchString)


        {
            Program.PrintCommandUse(ctx.User.Username, ctx.Message.Content);
            //await ctx.Channel.SendMessageAsync("Searching for " + searchString + "...").ConfigureAwait(false);


            WebClient Client = new WebClient();
            Client.Encoding = System.Text.Encoding.UTF8;
            string HTML = "";
            int snipIndex = -1;

            string OriginalFilter = string.Empty;
            OriginalFilter = "anime";

            if (HTML.Length > 250) return;

            string wordTemp = "";
            string URL = "";
            List<string> wordIDs = new List<string>() { };
            URL = "https://jpdb.io/prebuilt_decks?q=" + searchString;
            try
            {
                HTML = Client.DownloadString(new Uri(URL));
            }
            catch (Exception ex)
            {
                Program.PrintError(ex.Message);
                return;
            }

            snipIndex = HTML.IndexOf("30rem;\">") + 8;

            if (snipIndex == 7)
            {
                await ctx.RespondAsync("No content found UwU").ConfigureAwait(false);
                return;
            }
            wordTemp = HTML.Substring(snipIndex);
            HTML = wordTemp;

            snipIndex = wordTemp.IndexOf("<");
            HTML = wordTemp.Substring(snipIndex);
            string contentName = wordTemp.Substring(0, snipIndex);

            snipIndex = HTML.IndexOf("margin-top: 0.5rem;\">") + 22;
            wordTemp = HTML.Substring(snipIndex);
            snipIndex = wordTemp.IndexOf("/") + 1;
            wordTemp = wordTemp.Substring(snipIndex);
            snipIndex = wordTemp.IndexOf("\"");
            wordTemp = "https://jpdb.io/" + wordTemp.Substring(0, snipIndex);

            await ctx.RespondAsync("Found " + contentName + ":\n" + wordTemp).ConfigureAwait(false);

            int Frequency = 1;
            if (wordTemp.Contains(">") == false && wordTemp.Contains("<") == false & wordTemp.Contains("=") == false & wordTemp.Contains("-") == false)
            {
                try
                {
                    for (var I = 1; I <= Frequency; I++)
                        wordIDs.Add(wordTemp);
                }
                catch (Exception ex)
                {
                    Program.PrintError(ex.Message);
                    return;
                }
            }

            snipIndex = HTML.IndexOf("#a") + 3;
        }

        [Command("japantime")]
        [Cooldown(2, 10, CooldownBucketType.User)]
        [Description("Check the time in Japan")]
        public async Task japantime(CommandContext ctx)
        {
            Program.PrintCommandUse(ctx.User.Username, ctx.Message.Content);
            var info = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);
            String TimeInJapan = localTime.ToString("dd/MM/yyyy HH:mm:ss");
            var Kou = await ctx.Client.GetUserAsync(118408957416046593);
            if ((localTime.Hour > 21 || localTime.Hour < 5) && Kou.Presence.Status != DSharpPlus.Entities.UserStatus.Offline)
            {
                await ctx.RespondAsync("日本: " + TimeInJapan + $"\n{Kou.Username} is up late working on JPDB for us all <3").ConfigureAwait(false);
            }
            else
            {
                await ctx.RespondAsync("日本: " + TimeInJapan).ConfigureAwait(false);
            }
        }
    }


}
