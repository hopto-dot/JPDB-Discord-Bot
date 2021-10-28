using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

namespace JPDB_Bot.Commands
{
    public class Rules : BaseCommandModule
    {
        [Command("rule6")]
        [Cooldown(1, 20, CooldownBucketType.User)]
        [Hidden]
        [Description("Shows rule 6 for people that haven't read the rules :harold:")]
        public async Task rule6(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("If you've just joined the server do not ask us to translate something for you and then leave. We're not a free translation service. We're always happy to help people in our community, but if you only want to use us then don't bother.").ConfigureAwait(false);
        }
    }
}