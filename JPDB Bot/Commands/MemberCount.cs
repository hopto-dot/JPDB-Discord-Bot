using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Threading.Tasks;

namespace JPDB_Bot.Commands
{
    public class MemberCount : BaseCommandModule
    {
        [Command("members")]
        [Cooldown(1, 10, CooldownBucketType.User)]
        [Description("Check the time in Japan")]
        public async Task Count(CommandContext ctx)
        {
            var userCount = ctx.Guild.GetAllMembersAsync().Result.Count; //ctx.Guild.GetAllMembersAsync().Result.ToArray().Length;
            await ctx.Message.RespondAsync($"There are {userCount} members :)").ConfigureAwait(false);
        }
    }
}