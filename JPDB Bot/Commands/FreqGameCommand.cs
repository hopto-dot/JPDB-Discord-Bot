using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using JPDB_Bot.FreqGame;

namespace JPDB_Bot.Commands
{
    public class FreqGameCommand : BaseCommandModule
    {
        // These will be populated by dependency injection
        public Random Rng;
        public ConfigJson BotConfig;

        // Store a list of channels with games in progress, in case someone starts a game while a game is running
        private readonly Dictionary<DiscordChannel, FreqGame.FreqGame> GamesInProgress = new();

        [Command("stop")]
        [Hidden]
        [Cooldown(1, 10, CooldownBucketType.User)]
        [Description("Stop a frequency game in progress")]
        public async Task StopFrequencyGame(CommandContext ctx)
        {
            Program.printCommandUse(ctx.User.Username, ctx.Message.Content);

            DiscordChannel discordChannel = ctx.Channel;

            FreqGame.FreqGame gameInProgress = null;
            lock (GamesInProgress)
            {
                if (GamesInProgress.ContainsKey(discordChannel))
                {
                    gameInProgress = GamesInProgress[discordChannel];
                }
            }

            if (gameInProgress == null)
            {
                await discordChannel.SendMessageAsync("There is not a game in progress!").ConfigureAwait(false);
                return;
            }

            gameInProgress.RequestStop();
        }

        [Command("freqgame")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        [Description("Play a game where you guess which word is more frequent")]
        public async Task FrequencyGame(CommandContext ctx,
            [Description("Your jpdb username")] string jpdbUser = "",
            [Description("Number of rounds")] int rounds = 5)
        {
            // if (ctx.Guild.Name == "jpdb.io official")
            // {
            //     await ctx.Channel.SendMessageAsync($"The bot is currently being tested");
            //     return;
            // }

            Program.printCommandUse(ctx.User.Username, ctx.Message.Content);

            DiscordChannel discordChannel = ctx.Channel;

            FreqGame.FreqGame game = null;

            lock (GamesInProgress)
            {
                // If there is not a game in progress, then create a new game
                // and add it to the dictionary of games in progress.
                if (!GamesInProgress.ContainsKey(discordChannel))
                {
                    game = new FreqGame.FreqGame();
                    GamesInProgress[discordChannel] = game;
                }
            }

            if (game == null) // No new game because a game was already in progress.
            {
                await discordChannel.SendMessageAsync("A game is already in progress!").ConfigureAwait(false);
                return;
            }

            try
            {
                await game.RunGame(Rng, BotConfig, ctx, jpdbUser, rounds);
            }
            catch (FreqGame.TimeoutException)
            {
                await discordChannel.SendMessageAsync("Game timed out").ConfigureAwait(false);
            }
            catch (GameCancelledException)
            {
                await discordChannel.SendMessageAsync("The game has been aborted.").ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception from FreqGameCommand.FrequencyGame: \n" + e.Message + "\n" + e.Data);
                await discordChannel.SendMessageAsync("The game has failed with an exception.").ConfigureAwait(false);
                throw;
            }
            finally
            {
                lock(GamesInProgress)
                {
                    GamesInProgress.Remove(discordChannel);
                }
            }
        }
    }
}