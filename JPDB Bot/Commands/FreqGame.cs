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
using DiscordBot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JPDB_Bot.Commands
{
    public class FreqGame : BaseCommandModule
    {
        // These will be populated by dependency injection
        public Random random;

        [Command("freqgame")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        [Description("Play a game where you guess which word is more frequent")]
        public async Task guessgame(CommandContext ctx,
            [DescriptionAttribute("Your jpdb username")] string jpdbUser = "",
            [DescriptionAttribute("Number of rounds")] int rounds = 5)
        {
            if (ctx.Guild.Name == "jpdb.io official")
            {
                //await ctx.Channel.SendMessageAsync($"The bot is currently being tested");
                //return;
            }

            Program.PrintCommandUse(ctx.User.Username, ctx.Message.Content);

            double answerTime = 6;

            DiscordUser Player1 = ctx.Message.Author;

            await ctx.Channel.SendMessageAsync(
                $"Type \"!me [jpdb username]\" to play with {ctx.User.Username}, a jpdb username isn't required.\nType \"!start\" once you're all ready.");
            bool gameReady = false;
            List<gamePlayer> players = new List<gamePlayer>();
            //!freqgame
            string[] jpdbName = ctx.Message.Content.ToLower().Length > 10
                ? ctx.Message.Content.ToLower().Substring(10).Split(" ")
                : new string[] { ctx.Message.Content.ToLower() };
            gamePlayer newPlayer = new gamePlayer()
            {
                username = ctx.Message.Author.Username,
                jpdbUsername = ctx.Message.Content.ToLower().Length > 9 ? jpdbName[0] : string.Empty,
                //jpdbUsername = ctx.Message.Content.ToLower().Length > 9 ? ctx.Message.Content.ToLower().Substring(10) : string.Empty,
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
                                        return
                                            false; //if username in "!me [username]" is the same as one of the existing game players
                                    }
                                }
                            }
                        }

                        ;

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
                                string[] jpdbName = m.Content.ToLower().Substring(4).Split(" ");
                                gamePlayer newPlayer = new gamePlayer()
                                {
                                    username = m.Author.Username,
                                    jpdbUsername = jpdbName[0],
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
                Description = $"**Guess which word is more frequent ({rounds} Rounds)**\n\nParticipants:\n" +
                              string.Join("\n", playerNames),
                Color = DiscordColor.Red,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "Currently, usernames don't do anything.",
                }
            };
            await ctx.Channel.SendMessageAsync(embed: gameEmbed).ConfigureAwait(false);

            await Task.Delay(3000);

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

            if (rounds < 1 || rounds > 20)
            {
                rounds = 5;
            }

            string jpdbUsernames = string.Empty;
            foreach (gamePlayer player in players)
            {
                if (player.jpdbUsername != string.Empty)
                {
                    if (jpdbUsernames.Length != 0)
                    {
                        jpdbUsernames += ",";
                    }
                    else
                    {
                        jpdbUsernames = "&users=";
                    }

                    jpdbUsernames += player.jpdbUsername;
                }
            }

            int noReponse = 0;
            for (int round = 1; round <= rounds; round++)
            {
                WebRequest request; ///pick_words?count=2&spread=100&users=user1,user2,user3
                request = WebRequest.Create("https://jpdb.io/api/experimental/pick_words?count=2&spread=300" +
                                            jpdbUsernames);
                //request = WebRequest.Create("https://jpdb.io/api/experimental/pick_words?count=2&spread=300");//&users=user1,user2,user3";
                Program.PrintAPIUse("Freqgame",
                    "https://jpdb.io/api/experimental/pick_words?count=2&spread=300" + jpdbUsernames);

                request.Method = "GET";
                request.Headers["Authorization"] = "Bearer " + configJson.JPDBToken;

                HttpWebResponse response = null;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch
                {
                    await ctx.Channel
                        .SendMessageAsync(
                            "API request failed, this is usually because of an incorrect jpdb username.\nThe game has been aborted.")
                        .ConfigureAwait(false);
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
                }
                else
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

                int randomInteger = random.Next(1, 6);
                bool specialRound = false;
                answerTime = 6;
                if (randomInteger == 2 && round != 1)
                {
                    answerTime = 7;
                    specialRound = true;
                    await ctx.Channel
                        .SendMessageAsync("**BONUS POINTS ROUND!!!\nCorrectly answering scores you 3 points!**")
                        .ConfigureAwait(false);
                }

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
                    Title = $"Round {round}: Which word is more frequent?",
                    Description =
                        $"A = {wordA.vocabKanji} ({wordA.vocabReading})\nB = {wordB.vocabKanji} ({wordB.vocabReading})",
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

                var reactionResult =
                    await interactivity.CollectReactionsAsync(questionMessage, TimeSpan.FromSeconds(answerTime));
                var distinctResult = reactionResult.ToArray();

                if (distinctResult.Length == 0)
                {
                    noReponse += 1;
                }
                else
                {
                    noReponse = 0;
                }

                if (noReponse > 1)
                {
                    await ctx.Channel.SendMessageAsync("Game is inactive so the game has been stopped!")
                        .ConfigureAwait(false);
                    return;
                }

                //resetting the players' choices
                foreach (gamePlayer Person in players)
                {
                    Person.choice = "0";
                }


                foreach (var Reaction in distinctResult.ToArray())
                {
                    foreach (var User in Reaction.Users.ToArray())
                    {
                        if (User.Username != "JPDB Bot (Unofficial)")
                        {
                            bool playerExists = false;
                            foreach (gamePlayer Person in players)
                            {
                                if (Person.username == User.Username)
                                {
                                    playerExists = true;
                                }
                            }

                            if (playerExists == false)
                            {
                                if (User.Id == 118408957416046593)
                                {
                                    await ctx.Channel.SendMessageAsync($"God (-こう-) joined mid-game! :OO")
                                        .ConfigureAwait(false);
                                }
                                else
                                {
                                    await ctx.Channel.SendMessageAsync($"{User.Username} joined mid-game!")
                                        .ConfigureAwait(false);
                                }

                                gamePlayer addPlayer = new gamePlayer
                                {
                                    username = User.Username,
                                    jpdbUsername = string.Empty
                                };
                                players.Add(addPlayer);
                            }
                        }
                    }
                }

                foreach (var Reaction in distinctResult.ToArray())
                {
                    if (Reaction.Emoji.Name == "🇦")
                    {
                        foreach (var User in
                            Reaction.Users.ToArray()) //loop through all the users who've reacted with A
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

                            SkipA: ;
                        }
                    }
                    else if (Reaction.Emoji.Name == "🇧") //loop through all the users who've reacted with A
                    {
                        foreach (var User in
                            Reaction.Users.ToArray()) //loop through all the users who've reacted with A
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

                            SkipB: ;
                        }
                    }
                }


                List<string> correctPlayers = new List<string>();
                if (wordA.vocabFreq < wordB.vocabFreq) //if word A is more frequent
                {
                    await ctx.Channel
                        .SendMessageAsync(
                            $"{wordA.vocabKanji} ({wordA.vocabFreq}) was the correct answer, while {wordB.vocabKanji} has a frequency of {wordB.vocabFreq}!")
                        .ConfigureAwait(false);
                    foreach (gamePlayer Person in players)
                    {
                        if (Person.choice == "A")
                        {
                            if (specialRound == true)
                            {
                                Person.points += 3;
                            }
                            else
                            {
                                Person.points += 1;
                            }

                            correctPlayers.Add(Person.username);
                        }
                    }
                }
                else //if word B is more frequent
                {
                    await ctx.Channel
                        .SendMessageAsync(
                            $"{wordB.vocabKanji} ({wordB.vocabFreq}) was the correct answer, while {wordA.vocabKanji} has a frequency of {wordA.vocabFreq}!")
                        .ConfigureAwait(false);
                    foreach (gamePlayer Person in players)
                    {
                        if (Person.choice == "B")
                        {
                            if (specialRound == true)
                            {
                                Person.points += 3;
                            }
                            else
                            {
                                Person.points += 1;
                            }

                            correctPlayers.Add(Person.username);
                        }
                    }
                }

                if (correctPlayers.Count == 1)
                {
                    await ctx.Channel.SendMessageAsync($"{string.Join(" and ", correctPlayers)} got it right!")
                        .ConfigureAwait(false);
                }

                if (correctPlayers.Count == 1 && players.Count > 1)
                {
                    await ctx.Channel.SendMessageAsync($"Only {string.Join(" and ", correctPlayers)} got it right!")
                        .ConfigureAwait(false);
                }
                else if (correctPlayers.Count == 2)
                {
                    await ctx.Channel.SendMessageAsync($"{string.Join(" and ", correctPlayers)} got it right!")
                        .ConfigureAwait(false);
                }
                else if (correctPlayers.Count == players.Count && players.Count > 2)
                {
                    await ctx.Channel.SendMessageAsync("Everyone got it right!").ConfigureAwait(false);
                }
                else if (correctPlayers.Count > 2)
                {
                    await ctx.Channel.SendMessageAsync($"{string.Join(", ", correctPlayers)} got it right!")
                        .ConfigureAwait(false);
                }
                else if (correctPlayers.Count == 0 && players.Count > 3)
                {
                    await ctx.Channel.SendMessageAsync("How did no one get that right!?").ConfigureAwait(false);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync("No one got it right!").ConfigureAwait(false);
                }


                if (round != rounds)
                {
                    await Task.Delay(4000);
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
    }
}