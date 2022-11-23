using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.SlashCommands;
using System;
using System.Threading.Tasks;

namespace JPDB_Bot.Commands
{
    public class MemberCount : BaseCommandModule
    {
        [Command("members")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        [Description("Checks how many members the server has")]
        public async Task Count(CommandContext ctx)
        {
            Program.printCommandUse(ctx.Message.Author.Username, ctx.Message.Content);
            var userCount = ctx.Guild.GetAllMembersAsync().Result.Count; //ctx.Guild.GetAllMembersAsync().Result.ToArray().Length;
            await ctx.Message.RespondAsync($"There are {userCount} members :)").ConfigureAwait(false);
        }
    }

    public class MemberCountS : ApplicationCommandModule
    {
        [SlashCommand("members", "Checks how many members the server has")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        public async Task Count(InteractionContext ctx)
        {
            Program.printCommandUse(ctx.User.Username, "//members");
            var userCount = ctx.Guild.GetAllMembersAsync().Result.Count; //ctx.Guild.GetAllMembersAsync().Result.ToArray().Length;
            await ctx.CreateResponseAsync($"There are {userCount} members :)").ConfigureAwait(false);
        }
    }
}