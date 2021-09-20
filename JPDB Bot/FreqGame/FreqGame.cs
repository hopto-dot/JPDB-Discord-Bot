using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Interactivity.Extensions;
using Newtonsoft.Json.Linq;

namespace JPDB_Bot.FreqGame
{
    public class FreqGame
    {
        // Dependencies
        private Random Rng;
        private ConfigJson BotConfig;

        // Discord command context
        private CommandContext Ctx;

        // The state of the current game
        private int Rounds;
        private readonly Dictionary<DiscordUser, Player> GamePlayers;

        // Emoji
        private readonly DiscordEmoji EMOJI_A;
        private readonly DiscordEmoji EMOJI_B;

        // Outcomes from running one round
        enum RoundOutcome
        {
            Success,
            NoResponse,
            ApiError
        }

        public FreqGame(Random rng, ConfigJson botConfig, CommandContext ctx, string jpdbUser, int rounds)
        {
            Rng = rng;
            BotConfig = botConfig;
            Ctx = ctx;
            Rounds = rounds;
            GamePlayers = new Dictionary<DiscordUser, Player>();

            EMOJI_A = DiscordEmoji.FromName(Ctx.Client, ":regional_indicator_a:");
            EMOJI_B = DiscordEmoji.FromName(Ctx.Client, ":regional_indicator_b:");

            // Add the first player
            AddPlayer(ctx.Message.Author, jpdbUser);
        }

        public async Task RunGame()
        {
            await CollectPlayers();

            if (Rounds < 1 || Rounds > 20)
            {
                await Ctx.Channel.SendMessageAsync($"Playing 5 rounds instead of {Rounds} rounds")
                    .ConfigureAwait(false);
                Rounds = 5;
            }

            await ShowPlayerList();

            await Task.Delay(3000);

            var roundOutcome = await RunRounds();
            if (roundOutcome != RoundOutcome.Success)
                return;

            await ShowFinalScores();
        }

        private async Task CollectPlayers()
        {
            await Ctx.Channel.SendMessageAsync(
                $"Type \"!me [jpdb username]\" to play with {Ctx.User.Username}, a jpdb username isn't required.\n" +
                "Type \"!start\" once you're all ready.");

            try
            {
                while (true)
                {
                    string[] commands = new[] { "!me", "!start" };

                    InteractivityResult<DiscordMessage> result =
                        await Ctx.Channel.GetNextMessageAsync(message =>
                            {
                                string[] parts = message.Content.Split();
                                return parts.Length > 0 && commands.Contains(parts[0]);
                            })
                            .ConfigureAwait(false);

                    if (result.TimedOut)
                    {
                        throw new TimeoutException();
                    }

                    DiscordMessage message = result.Result;
                    string[] parts = message.Content.Split();

                    if (parts.Length > 0)
                    {
                        switch (parts[0])
                        {
                            case "!me":
                                DiscordUser user = message.Author;
                                string jpdbUsername = parts.Length > 1 ? parts[1] : "";

                                await RunMeCommand(user, jpdbUsername);

                                break;
                            case "!start":
                                return;
                        }
                    }
                }
            }
            catch
            {
                throw new TimeoutException();
            }
        }

        private async Task RunMeCommand(DiscordUser user, string jpdbUsername)
        {
            bool alreadyJoined = GamePlayers.ContainsKey(user);

            AddPlayer(user, jpdbUsername);

            if (alreadyJoined)
            {
                if (jpdbUsername == "")
                {
                    await Ctx.Channel.SendMessageAsync($"Username for {user.Username} updated to (none)"
                    ).ConfigureAwait(false);
                }
                else
                {
                    await Ctx.Channel.SendMessageAsync($"Username for {user.Username} updated to {jpdbUsername}"
                    ).ConfigureAwait(false);
                }
            }
            else
            {
                if (jpdbUsername == "")
                {
                    await Ctx.Channel.SendMessageAsync($"{user.Username} joined the game"
                    ).ConfigureAwait(false);
                }
                else
                {
                    await Ctx.Channel.SendMessageAsync($"{user.Username} ({jpdbUsername}) joined the game"
                    ).ConfigureAwait(false);
                }
            }
        }

        private void AddPlayer(DiscordUser user, string jpdbUsername)
        {
            GamePlayers[user] = new Player(user.Username, jpdbUsername);
        }

        private async Task ShowPlayerList()
        {
            List<string> playerNames = GetPlayerListForDisplay();

            await Ctx.Channel.SendMessageAsync(string.Join(" 対 ", playerNames)).ConfigureAwait(false);

            var gameEmbed = new DiscordEmbedBuilder
            {
                Title = $"Freq guessing game",
                Description = $"**Guess which word is more frequent ({Rounds} Rounds)**\n\nParticipants:\n" +
                              string.Join("\n", playerNames),
                Color = DiscordColor.Red,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "Currently, usernames don't do anything.",
                }
            };
            await Ctx.Channel.SendMessageAsync(embed: gameEmbed).ConfigureAwait(false);
        }

        private List<string> GetPlayerListForDisplay()
        {
            List<string> playerNames = new();

            foreach (KeyValuePair<DiscordUser, Player> pair in GamePlayers)
            {
                Player player = pair.Value;
                string playerName = player.JpdbUsername;
                if (playerName == "")
                {
                    playerName = "*No jpdb name*";
                }

                playerNames.Add(player.Username + $" ({playerName})");
            }

            return playerNames;
        }

        private async Task<RoundOutcome> RunRounds()
        {
            int noResponseCount = 0;
            for (int round = 1; round <= Rounds; round++)
            {
                var outcome = await RunOneRound(round);
                switch (outcome)
                {
                    case RoundOutcome.ApiError:
                        return outcome;
                    case RoundOutcome.NoResponse:
                    {
                        noResponseCount++;
                        if (noResponseCount < 2) break;
                        await Ctx.Channel.SendMessageAsync("Game is inactive so the game has been stopped!")
                            .ConfigureAwait(false);
                        return outcome;
                    }
                    case RoundOutcome.Success:
                        noResponseCount = 0;
                        break;
                }

                if (round < Rounds) await Task.Delay(4000);
            }

            return RoundOutcome.Success;
        }

        private async Task<RoundOutcome> RunOneRound(int round)
        {
            string responseFromServer = await GetWordsFromServer();
            if (responseFromServer == "")
            {
                return RoundOutcome.ApiError;
            }

            Vocabulary[] words = ParseVocabulary(responseFromServer);
            Console.WriteLine("Parsed words.");

            Question question = new(words[0], words[1], Rng);

            bool bonusRound = false;
            double answerTime = 6;
            if (round != 1 && Rng.Next(1, 6) == 2)
            {
                bonusRound = true;
                answerTime = 7;
                await Ctx.Channel
                    .SendMessageAsync("**BONUS POINTS ROUND!!!\nCorrectly answering scores you 3 points!**")
                    .ConfigureAwait(false);
            }

            Dictionary<DiscordUser, List<DiscordEmoji>> playerReactions =
                await AskAndWaitForReactions(round, question.WordA, question.WordB, answerTime);

            if (playerReactions.Count == 0)
                return RoundOutcome.NoResponse;

            await CheckForNewlyJoinedPlayers(playerReactions);

            int pointsAwarded = bonusRound ? 3 : 1;

            List<DiscordUser> winners = AwardPoints(question, playerReactions, pointsAwarded);

            await ReportRoundResults(question, winners);

            return RoundOutcome.Success;
        }

        private async Task<string> GetWordsFromServer()
        {
            // pick_words?count=2&spread=100&users=user1,user2,user3
            string url = "https://jpdb.io/api/experimental/pick_words?count=2&spread=300";
            string playerList = string.Join(",",
                GamePlayers.Where(pair => pair.Value.JpdbUsername.Length > 0).Select(pair => pair.Value.JpdbUsername));
            if (playerList != "")
            {
                url += "&users=" + playerList;
            }

            WebRequest request = WebRequest.Create(url);
            Program.PrintAPIUse("Freqgame", url);

            request.Method = "GET";
            request.Headers["Authorization"] = "Bearer " + BotConfig.JPDBToken;

            WebResponse response;
            try
            {
                response = await request.GetResponseAsync();
            }
            catch (Exception e)
            {
                await Ctx.Channel.SendMessageAsync(
                        "API request failed, this is usually because of an incorrect jpdb username.\nThe game has been aborted.")
                    .ConfigureAwait(false);
                Console.WriteLine(e.Message);
                Console.WriteLine("url is " + url);
                throw;
            }

            //Console.WriteLine(response.StatusDescription);

            //Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            //Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            //Read the content.
            string responseFromServer = await reader.ReadToEndAsync();
            //Display the content.
            Console.WriteLine(responseFromServer);
            //Clean up the streams and the response.
            reader.Close();
            response.Close();

            return responseFromServer;
        }

        private static Vocabulary[] ParseVocabulary(string responseFromServer)
        {
            responseFromServer = responseFromServer.Replace(@"\" + "\"word" + @"\" + "\"", ""); //  \"words\":
            JToken[] fullJson = JObject.Parse(responseFromServer).SelectToken("words").ToArray();

            Console.WriteLine("Parsed JSON response");

            Newtonsoft.Json.Linq.JToken token1 = fullJson[0];
            Newtonsoft.Json.Linq.JToken token2 = fullJson[1];

            Vocabulary[] words = new[]
            {
                new Vocabulary
                {
                    vocabKanji = token1.SelectToken("spelling").ToString(),
                    vocabReading = token1.SelectToken("reading").ToString(),
                    vocabFreq = token1.SelectToken("vrank").ToObject<int>(),
                },
                new Vocabulary
                {
                    vocabKanji = token2.SelectToken("spelling").ToString(),
                    vocabReading = token2.SelectToken("reading").ToString(),
                    vocabFreq = token2.SelectToken("vrank").ToObject<int>(),
                }
            };

            return words;
        }

        private async Task<Dictionary<DiscordUser, List<DiscordEmoji>>> AskAndWaitForReactions(
            int round, Vocabulary wordA, Vocabulary wordB, double answerTime)
        {
            List<string> playerPoints = GamePlayers.OrderByDescending(pair => pair.Value.Points).Select(
                pair => $"{pair.Value.Username}: {pair.Value.Points}").ToList();

            DiscordEmbedBuilder gameEmbed = new DiscordEmbedBuilder
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

            var questionMessage = await Ctx.Channel.SendMessageAsync(embed: gameEmbed).ConfigureAwait(false);
            await Task.Delay(350);
            await questionMessage.CreateReactionAsync(EMOJI_A);
            await Task.Delay(350);
            await questionMessage.CreateReactionAsync(EMOJI_B);

            var interactivity = Ctx.Client.GetInteractivity();
            var reactionResult = await interactivity.CollectReactionsAsync(
                questionMessage, TimeSpan.FromSeconds(answerTime));

            Dictionary<DiscordUser, List<DiscordEmoji>> reactionsDict = GetValidReactionsByUser(reactionResult);

            return reactionsDict;
        }

        private Dictionary<DiscordUser, List<DiscordEmoji>> GetValidReactionsByUser(
            ReadOnlyCollection<Reaction> reactions)
        {
            Dictionary<DiscordUser, List<DiscordEmoji>> reactionsDict = new();

            foreach (var reaction in reactions)
            {
                if (reaction.Emoji == EMOJI_A || reaction.Emoji == EMOJI_B)
                {
                    foreach (DiscordUser user in reaction.Users)
                    {
                        if (user.IsBot) continue; // skip reactions from bots
                        if (!reactionsDict.ContainsKey(user))
                            reactionsDict.Add(user, new List<DiscordEmoji>());
                        reactionsDict[user].Add(reaction.Emoji);
                    }
                }
            }

            return reactionsDict;
        }

        private async Task CheckForNewlyJoinedPlayers(Dictionary<DiscordUser, List<DiscordEmoji>> reactionsDict)
        {
            foreach ((DiscordUser user, List<DiscordEmoji> emoji) in reactionsDict)
            {
                if (!GamePlayers.ContainsKey(user))
                {
                    if (user.Id == 118408957416046593)
                    {
                        await Ctx.Channel.SendMessageAsync($"God (-こう-) joined mid-game! :OO").ConfigureAwait(false);
                    }
                    else
                    {
                        await Ctx.Channel.SendMessageAsync($"{user.Username} joined mid-game!").ConfigureAwait(false);
                    }

                    AddPlayer(user, "");
                }
            }
        }

        private List<DiscordUser> AwardPoints(Question question,
            Dictionary<DiscordUser, List<DiscordEmoji>> playerReactions, int pointsAwarded)
        {
            DiscordEmoji correctEmoji = question.CorrectWord == question.WordA ? EMOJI_A : EMOJI_B;
            DiscordEmoji wrongEmoji = correctEmoji == EMOJI_B ? EMOJI_A : EMOJI_B;
            List<DiscordUser> winners = new();
            foreach ((DiscordUser user, List<DiscordEmoji> emoji) in playerReactions)
            {
                if (emoji.Contains(correctEmoji) && !emoji.Contains(wrongEmoji))
                {
                    winners.Add(user);
                    GamePlayers[user].Points += pointsAwarded;
                }
            }

            return winners;
        }

        private async Task ReportRoundResults(Question question, List<DiscordUser> winners)
        {
            Vocabulary correctWord = question.CorrectWord;
            Vocabulary wrongWord = question.WrongWord;
            await Ctx.Channel.SendMessageAsync(
                    $"{correctWord.vocabKanji} ({correctWord.vocabFreq}) was the correct answer, " +
                    $"while {wrongWord.vocabKanji} has a frequency of {wrongWord.vocabFreq}!")
                .ConfigureAwait(false);

            if (winners.Count == GamePlayers.Count && GamePlayers.Count > 2)
            {
                await Ctx.Channel.SendMessageAsync("Everyone got it right!").ConfigureAwait(false);
            }
            else if (winners.Count == 0)
            {
                if (GamePlayers.Count > 2)
                {
                    await Ctx.Channel.SendMessageAsync("How did no one get that right!?").ConfigureAwait(false);
                }
                else
                {
                    await Ctx.Channel.SendMessageAsync("No one got it right!").ConfigureAwait(false);
                }
            }
            else
            {
                List<string> winnerNames = winners.Select(winner => winner.Username).ToList();
                await Ctx.Channel.SendMessageAsync($"{string.Join(" and ", winnerNames)} got it right!")
                    .ConfigureAwait(false);
            }
        }


        private async Task ShowFinalScores()
        {
            List<string> playerPoints = GamePlayers.OrderByDescending(pair => pair.Value.Points).Select(
                pair => $"{pair.Value.Username}: {pair.Value.Points}").ToList();

            var gameEmbed = new DiscordEmbedBuilder
            {
                Title = "Game result:",
                Description = "**Points:**",
                Color = DiscordColor.Red,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = string.Join("\n", playerPoints)
                }
            };
            await Ctx.Channel.SendMessageAsync(embed: gameEmbed).ConfigureAwait(false);
        }
    }
}