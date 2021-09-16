using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Threading.Tasks;

namespace JPDB_Bot.Commands
{
    public class JapanTime : BaseCommandModule
    {
        [Command("japantime")]
        [Cooldown(2, 10, CooldownBucketType.User)]
        [Description("Check the time in Japan")]
        public async Task ShowJapanTime(CommandContext ctx)
        {
            Program.PrintCommandUse(ctx.User.Username, ctx.Message.Content);
            var info = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);
            String timeInJapan = localTime.ToString("dd/MM/yyyy HH:mm:ss");
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
    }
}