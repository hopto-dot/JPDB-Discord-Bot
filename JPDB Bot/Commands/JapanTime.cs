using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Threading.Tasks;

namespace JPDB_Bot.Commands
{
    public class JapanTime : BaseCommandModule
    {
        //[Command("time")]
        //[Cooldown(2, 6, CooldownBucketType.User)]
        //[Description("Check the time in Japan")]
        //[Hidden]
        //[Aliases("koutime")]
        //public async Task time(CommandContext ctx, string timeZone)
        //{
        //    switch (timeZone)
        //    {
        //        case "japantime":

        //    }


        //}


        [Command("japantime")]
        [Cooldown(2, 10, CooldownBucketType.User)]
        [Description("Check the time in Japan")]
        [Hidden]
        [Aliases("koutime", "mootime", "cowtime", "nihontime", "nihonjikan")]
        public async Task japantime(CommandContext ctx)
        {
            Program.printCommandUse(ctx.User.Username, ctx.Message.Content);
            var info = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);
            String timeInJapan = localTime.ToString("yyyy/MM/dd HH:mm:ss");
            try
            {
                var userKou = await ctx.Client.GetUserAsync(118408957416046593);
                if ((localTime.Hour > 21 || localTime.Hour < 5) &&
                    userKou.Presence.Status != DSharpPlus.Entities.UserStatus.Offline)
                {
                    await ctx.RespondAsync("日本: " + timeInJapan +
                                           $"\n{userKou.Username} is up late working on JPDB for us all <3")
                        .ConfigureAwait(false);
                }
                else
                {
                    await ctx.RespondAsync("日本: " + timeInJapan).ConfigureAwait(false);
                }
            }
            catch
            {
                await ctx.RespondAsync("日本: " + timeInJapan).ConfigureAwait(false);
            }
        }

        [Command("flynttime")]
        [Aliases("esttime", "utc-5time", "utc-5", "flunttime", "cattime", "500cardsadaytime")]
        [Cooldown(2, 10, CooldownBucketType.User)]
        [Hidden]
        [Description("Check Flynt's time")]
        public async Task flynttime(CommandContext ctx)
        {
            Program.printCommandUse(ctx.User.Username, ctx.Message.Content);
            var info = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);
            String flyntTime = localTime.ToString("dd/MM/yyyy HH:mm:ss");
            
            await ctx.RespondAsync($"EST: {flyntTime}").ConfigureAwait(false);
        }

        [Command("alemaxtime")]
        [Cooldown(2, 10, CooldownBucketType.User)]
        [Description("Check the time in Germany")]
        [Hidden]
        [Aliases("germanytime", "deutschtime", "germantime", "cesttime", "cettime", "schnitzeltime", "alextime", "aletime", "n6time", "certifiedn6time")]
        public async Task alemaxtime(CommandContext ctx)
        {
            Program.printCommandUse(ctx.User.Username, ctx.Message.Content);
            var info = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);
            String alemaxTime = localTime.ToString("dd/MM/yyyy HH:mm:ss");

            await ctx.RespondAsync($"CET: {alemaxTime}").ConfigureAwait(false);
        }

        [Command("jawgboitime")]
        [Hidden]
        [Aliases("moekyunkyuntime" , "moekyuntime", "moetime", "uktime", "britishtime", "utc+1", "utc+1time", "jawtime", "jawgtime", "rebekahtime", "kitsunetime", "foxtime", "queentime", "bri'ishtime", "briishtime")]
        [Cooldown(2, 10, CooldownBucketType.User)]
        [Description("Check the time in the UK")]
        public async Task jawgboitime(CommandContext ctx)
        {
            Program.printCommandUse(ctx.User.Username, ctx.Message.Content);
            var info = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);
            String jawgboiTime = localTime.ToString("dd/MM/yyyy HH:mm:ss");

            await ctx.RespondAsync($"GMT: {jawgboiTime}").ConfigureAwait(false);
        }
    }
}