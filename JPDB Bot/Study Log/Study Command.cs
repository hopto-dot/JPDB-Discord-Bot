using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

namespace JPDB_Bot.StudyLog
{
    public class Study_Command : BaseCommandModule
    {
        [Command("study")]
        [Cooldown(1, 20, CooldownBucketType.User)]
        [Hidden]
        [Description("Shows rule 6 for people that haven't read the rules :harold:")]
        public async Task study(CommandContext ctx, string argument1, string argument2 = "", string argument3 = "")
        {
            //if '!study log' or !log request
            if (argument1 == "log" && (argument2 == "" || argument2 == "request"))
            {
                try
                {
                    await Bot.studyUserLogRequest(ctx.User, ctx);
                    await ctx.RespondAsync("Successfully DMed you your study log UwU").ConfigureAwait(false);
                }
                catch
                {
                    await ctx.RespondAsync("Failed to DM you your study log \\*cries in japanese\\*").ConfigureAwait(false);
                }
            }

            //if '!study log delete
            if (argument1 == "log" && argument2 == "delete")
            {
                try
                {
                    await Bot.studyUserLogDelete(ctx.User, ctx);
                    await ctx.RespondAsync("Successfully deleted your study log UwU").ConfigureAwait(false);
                }
                catch
                {
                    await ctx.RespondAsync("Failed to deleted your study log \\*cries in japanese\\*").ConfigureAwait(false);
                }
            }

            //if !study define [definition] [type]
            if (argument1 == "define")
            {
                if (argument2 == "") { await ctx.RespondAsync("You must specify the type of immersion you are defining.\n[]").ConfigureAwait(false); }
            }

            //if '!study [type of study]



        }
    }
}