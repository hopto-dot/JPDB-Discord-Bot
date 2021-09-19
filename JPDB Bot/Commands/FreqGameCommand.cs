using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace JPDB_Bot.Commands
{
    public class FreqGameCommand : BaseCommandModule
    {
        // These will be populated by dependency injection
        public Random Rng;
        public ConfigJson BotConfig;

        // Store a list of channels with games in progress, in case someone starts a game while a game is running
        private readonly Dictionary<DiscordChannel, FreqGame.FreqGame> GamesInProgress = new();

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

            Program.PrintCommandUse(ctx.User.Username, ctx.Message.Content);

            DiscordChannel discordChannel = ctx.Channel;

            if (GamesInProgress.ContainsKey(discordChannel))
            {
                await discordChannel.SendMessageAsync("A game is already in progress!").ConfigureAwait(false);
                return;
            }

            FreqGame.FreqGame game = new(Rng, BotConfig, ctx, jpdbUser, rounds);
            GamesInProgress[discordChannel] = game;
            try
            {
                await (game.RunGame());
            }
            catch (FreqGame.TimeoutException)
            {
                await ctx.Channel.SendMessageAsync("Game timed out").ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            finally
            {
                GamesInProgress.Remove(discordChannel);
            }
        }
    }
}