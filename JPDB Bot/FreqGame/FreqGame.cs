using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Newtonsoft.Json.Linq;

namespace JPDB_Bot.FreqGame
{
    public class FreqGame
    {
        public const int MaxRounds = 20;
        public const int DelayBetweenRoundsMillis = 4000;
        public const int DelayBeforeFirstRoundMillis = 3000;
        public const int DelayReactionsToAvoidRateLimitMillis = 350;
        public const int AnswerTimeoutMillis = 6000;
        public const int BonusRoundAnswerTimeoutMillis = 7000;

        // Dependencies
        private Random Rng;
        private ConfigJson BotConfig;

        // Discord command context
        private CommandContext Ctx;

        // The state of the current game
        private int Rounds;
        private Dictionary<DiscordUser, Player> GamePlayers;

        // Emoji
        private DiscordEmoji EMOJI_A;
        private DiscordEmoji EMOJI_B;

        // Stop flag
        public bool IsStopRequested { get; private set; }

        // Outcomes from running one round
        enum RoundOutcome
        {
            Success,
            NoResponse,
            ApiError
        }

        public FreqGame()
        {
            // Note: This constructor will be called from inside a lock.
            // Minimal initialization only!
            IsStopRequested = false;
        }

        public void RequestStop()
        {
            IsStopRequested = true;
        }

        private void CheckForCancellation()
        {
            if (IsStopRequested)
            {
                throw new GameCancelledException();
            }
        }

        private async Task DelayWithAbort(int millisecondsDelay)
        {
            CheckForCancellation();

            // Wait for a stop message. The timeout is equal to the requested delay.
            InteractivityResult<DiscordMessage> result = await Ctx.Channel.GetNextMessageAsync(
                    message =>
                    {
                        string[] parts = message.Content.Split();
                        return parts.Length > 0 && parts[0] == "!stop";
                    }, TimeSpan.FromMilliseconds(millisecondsDelay))
                .ConfigureAwait(false);

            if (result.TimedOut)
            {
                return; // A stop was not requested during the delay
            }

            DiscordMessage message = result.Result;
            string[] parts = message.Content.Split();
            if (parts.Length > 0 && parts[0] == "!stop")
            {
                RequestStop();
                CheckForCancellation();
            }
        }

        public async Task RunGame(Random rng, ConfigJson botConfig, CommandContext ctx, string jpdbUser, int rounds)
        {
            InitializeGame(rng, botConfig, ctx, jpdbUser, rounds);

            await CollectPlayers();

            CheckForCancellation();

            if (Rounds is < 1 or > MaxRounds)
            {
                await Ctx.Channel.SendMessageAsync($"Playing 5 rounds instead of {Rounds} rounds")
                    .ConfigureAwait(false);
                Rounds = 5;
            }

            (DiscordMessage playerListMessage, DiscordEmbedBuilder gameEmbed) = await ShowPlayerList();

            try
            {
                await DelayWithAbort(DelayBeforeFirstRoundMillis);
                CheckForCancellation();
            }
            catch (GameCancelledException)
            {
                // Update the embed
                await playerListMessage.ModifyAsync(
                    gameEmbed
                        .WithColor(DiscordColor.Gray)
                        .WithFooter("ABORTED!")
                        .Build());

                throw; // Re-throw the exception to actually stop the game
            }

            // Update the embed to gray instead of red.
            // The latest embed will be red, and a new one will be posted at the beginning of the first round.
            await playerListMessage.ModifyAsync(
                gameEmbed
                    .WithColor(DiscordColor.Gray)
                    .Build());

            RoundOutcome roundOutcome = await RunRounds();
            if (roundOutcome != RoundOutcome.Success)
                return;

            await ShowFinalScores();
        }

        private void InitializeGame(Random rng, ConfigJson botConfig, CommandContext ctx, string jpdbUser, int rounds)
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

        private async Task CollectPlayers()
        {
            CheckForCancellation();
            await Ctx.Channel.SendMessageAsync(
                $"Type \"!me [jpdb username]\" to play with {Ctx.User.Username}, a jpdb username isn't required.\n" +
                "Type \"!start\" once you're all ready.");

            while (true)
            {
                CheckForCancellation();

                string[] commands = new[] { "!me", "!start", "!stop" };

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
                        case "!stop":
                            RequestStop();
                            CheckForCancellation();
                            return;
                    }
                }
            }
        }

        private async Task RunMeCommand(DiscordUser user, string jpdbUsername)
        {
            CheckForCancellation();

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

        private async Task<(DiscordMessage playerListMessage, DiscordEmbedBuilder gameEmbed)> ShowPlayerList()
        {
            CheckForCancellation();

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
            var playerListMessage = await Ctx.Channel.SendMessageAsync(embed: gameEmbed).ConfigureAwait(false);
            return (playerListMessage, gameEmbed);
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
                        await Ctx.Channel.SendMessageAsync("Game is inactive so the game has been aborted!")
                            .ConfigureAwait(false);
                        return outcome;
                    }
                    case RoundOutcome.Success:
                        noResponseCount = 0;
                        break;
                }

                if (round < Rounds)
                {
                    await DelayWithAbort(DelayBetweenRoundsMillis);
                }

                CheckForCancellation();
            }

            return RoundOutcome.Success;
        }

        private async Task<RoundOutcome> RunOneRound(int round)
        {
            CheckForCancellation();

            string responseFromServer = await GetWordsFromServer();
            if (responseFromServer == "")
            {
                return RoundOutcome.ApiError;
            }

            Vocabulary[] words = ParseVocabulary(responseFromServer);
            Console.WriteLine("Parsed words.");

            Question question = new(words[0], words[1], Rng);

            bool bonusRound = false;
            int answerTimeMillis = AnswerTimeoutMillis;
            if (round != 1 && Rng.Next(1, 7) == 20) // Bonus rounds disabled
            {
                bonusRound = true;
                answerTimeMillis = BonusRoundAnswerTimeoutMillis;
                await Ctx.Channel
                    .SendMessageAsync("**Bonus Points Round!\nCorrectly answering scores you 2 points!**")
                    .ConfigureAwait(false);
            }

            Dictionary<DiscordUser, List<DiscordEmoji>> playerReactions =
                await AskAndWaitForReactions(round, question, answerTimeMillis);

            if (playerReactions.Count == 0)
                return RoundOutcome.NoResponse;

            await CheckForNewlyJoinedPlayers(playerReactions);

            int pointsAwarded = bonusRound ? 2 : 1;

            List<DiscordUser> winners = AwardPoints(question, playerReactions, pointsAwarded);

            await ReportRoundResults(question, winners);

            return RoundOutcome.Success;
        }

        private async Task<string> GetWordsFromServer()
        {
            CheckForCancellation();

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
            int round, Question question, int answerTimeMillis)
        {
            CheckForCancellation();

            Vocabulary wordA = question.WordA;
            Vocabulary wordB = question.WordB;

            List<string> playerPoints = GamePlayers.OrderByDescending(pair => pair.Value.Points).Select(
                pair => $"{pair.Value.Username}: {pair.Value.Points}").ToList();
            string playerPointsText = string.Join("\n", playerPoints);

            DiscordEmbedBuilder gameEmbed = new()
            {
                Title = $"Round {round}: Which word is more frequent?",
                Description =
                    $"A = {wordA.vocabKanji} ({wordA.vocabReading})\nB = {wordB.vocabKanji} ({wordB.vocabReading})",
                Color = DiscordColor.Red,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = playerPointsText
                }
            };

            // -------------------------------------------
            // Reactions are recorded using event handlers
            // -------------------------------------------

            // Show the embed with the question
            DiscordMessage questionMessage =
                await Ctx.Channel.SendMessageAsync(embed: gameEmbed.Build()).ConfigureAwait(false);

            // This will store the reactions for each player
            Dictionary<DiscordUser, List<DiscordEmoji>> reactionsDict = new();

            // Local event handler to record when a reaction is added
            Task ClientOnMessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs args)
            {
                return AddMessageReaction(questionMessage, reactionsDict, sender, args);
            }

            // Local event handler to record when a reaction is removed
            Task ClientOnMessageReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs args)
            {
                return RemoveMessageReaction(questionMessage, reactionsDict, sender, args);
            }

            // Register the event handlers
            Ctx.Client.MessageReactionAdded += ClientOnMessageReactionAdded;
            Ctx.Client.MessageReactionRemoved += ClientOnMessageReactionRemoved;

            try
            {
                // Add the possible answers as reactions
                await Task.Delay(DelayReactionsToAvoidRateLimitMillis);
                await questionMessage.CreateReactionAsync(EMOJI_A);
                await Task.Delay(DelayReactionsToAvoidRateLimitMillis);
                await questionMessage.CreateReactionAsync(EMOJI_B);

                // Give the players time to respond
                await DelayWithAbort(answerTimeMillis);
            }
            catch (GameCancelledException)
            {
                // Update the embed
                await questionMessage.ModifyAsync(
                    gameEmbed
                        .WithColor(DiscordColor.Gray)
                        .WithFooter("ABORTED!")
                        .Build());

                throw; // Re-throw the exception to actually stop the game
            }
            finally // Make sure the event handlers are unregistered!
            {
                // Unregister the event handlers
                Ctx.Client.MessageReactionAdded -= ClientOnMessageReactionAdded;
                Ctx.Client.MessageReactionRemoved -= ClientOnMessageReactionRemoved;
                //Console.WriteLine("AskAndWaitForReactions - Event handlers unregistered");
            }

            List<string> emojisByUser = new();
            foreach ((DiscordUser user, List<DiscordEmoji> discordEmojis) in reactionsDict)
            {
                List<string> emojiString = discordEmojis.Select(emoji =>
                {
                    if (emoji == EMOJI_A)
                        return "A";
                    if (emoji == EMOJI_B)
                        return "B";
                    return "";
                }).ToList();
                emojisByUser.Add(user.Username + " : " + string.Join(", ", emojiString));
            }

            await questionMessage.ModifyAsync(gameEmbed
                .WithColor(DiscordColor.Gray)
                .WithFooter("Responses: \n" + string.Join("\n", emojisByUser))
                .Build());

            // await Ctx.Channel.SendMessageAsync("Responses: \n" + string.Join("\n", emojisByUser))
            //     .ConfigureAwait(false);

            return reactionsDict;
        }

        private Task AddMessageReaction(
            DiscordMessage questionMessage,
            Dictionary<DiscordUser, List<DiscordEmoji>> reactionsDict,
            DiscordClient sender,
            MessageReactionAddEventArgs args)
        {
            if (args.Message == questionMessage)
            {
                DiscordUser user = args.User;
                if (!user.IsBot)
                {
                    DiscordEmoji emoji = args.Emoji;
                    if (emoji == EMOJI_A || emoji == EMOJI_B)
                    {
                        lock (reactionsDict)
                        {
                            if (!reactionsDict.ContainsKey(user))
                                reactionsDict.Add(user, new List<DiscordEmoji>());
                            reactionsDict[user].Add(emoji);
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

        private Task RemoveMessageReaction(
            DiscordMessage questionMessage,
            Dictionary<DiscordUser, List<DiscordEmoji>> reactionsDict,
            DiscordClient sender,
            MessageReactionRemoveEventArgs args)
        {
            if (args.Message == questionMessage)
            {
                DiscordUser user = args.User;
                if (!user.IsBot)
                {
                    lock (reactionsDict)
                    {
                        if (reactionsDict.ContainsKey(user))
                            reactionsDict[user].Remove(args.Emoji);
                    }
                }
            }

            return Task.CompletedTask;
        }

        private async Task CheckForNewlyJoinedPlayers(Dictionary<DiscordUser, List<DiscordEmoji>> reactionsDict)
        {
            CheckForCancellation();

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
            CheckForCancellation();

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
            CheckForCancellation();

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