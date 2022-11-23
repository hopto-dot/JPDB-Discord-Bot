using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using DSharpPlus.SlashCommands;

namespace JPDB_Bot.Commands
{
    public class Rules : BaseCommandModule
    {
        [Command("rule6")]
        [Cooldown(1, 20, CooldownBucketType.User)]
        [Description("Shows rule 6 for people that haven't read the rules")]
        public async Task rule6(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("If you've just joined the server do not ask us to translate something for you and then leave. We're not a free translation service. We're always happy to help people in our community, but if you only want to use us then don't bother.").ConfigureAwait(false);
        }

        [Command("rule34")]
        [Cooldown(1, 30, CooldownBucketType.User)]
        [Hidden]
        public async Task rule34(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Rule 34 doesn't exist on this server, seriously.").ConfigureAwait(false);
        }

        [Command("helprequest")]
        [Aliases("context", "requesthelp")]
        [Cooldown(1, 30, CooldownBucketType.User)]
        public async Task helprequest(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("When requesting help on a specific usage, please include, as applicable, what the source of the usage is, as well as either a direct copy of the text or a screenshot if not copyable of the usage and surrounding context. Japanese is a heavily context-based language and you will likely not get good answers if the responder has to assume key details.").ConfigureAwait(false);
        }
    }

    public class RulesS : ApplicationCommandModule
    {
        [SlashCommand("rule6", "Shows rule 6 for people that haven't read the rules")]
        [Cooldown(1, 20, CooldownBucketType.User)]
        public async Task rule6(InteractionContext ctx)
        {
            await ctx.Channel.SendMessageAsync("If you've just joined the server do not ask us to translate something for you and then leave. We're not a free translation service. We're always happy to help people in our community, but if you only want to use us then don't bother.").ConfigureAwait(false);
            await ctx.CreateResponseAsync("Message was sent", true);
        }

        [SlashCommand("helprequest", "Sends an explanation in chat asking for context to make answering a question easier")]
        [Cooldown(1, 30, CooldownBucketType.User)]
        public async Task helprequest(InteractionContext ctx)
        {
            await ctx.Channel.SendMessageAsync("When requesting help on a specific usage, please include, as applicable, what the source of the usage is, as well as either a direct copy of the text or a screenshot if not copyable of the usage and surrounding context. Japanese is a heavily context-based language and you will likely not get good answers if the responder has to assume key details.").ConfigureAwait(false);
            await ctx.CreateResponseAsync("Message was sent", true);
        }
    }
}