using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity.Enums;

namespace JPDB_Bot.Commands
{
    public class JapanTime : BaseCommandModule
    {
        [Command("japantime")]
        [Cooldown(2, 10, CooldownBucketType.User)]
        [Description("Check the time in Japan")]
        [Hidden]
        [Aliases("koutime", "mootime", "cowtime", "nihontime", "nihonjikan", "sushitime")]
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
                if ((localTime.Hour > 21 || localTime.Hour < 4) &&
                    userKou.Presence.Status != DSharpPlus.Entities.UserStatus.Offline)
                {
                    await ctx.RespondAsync("日本: " + timeInJapan +
                                           $"\n{userKou.Username} is up late working on JPDB for us all <3")
                        .ConfigureAwait(false);
                }
                else if ((localTime.Hour > 21 || localTime.Hour < 5) &&
                    userKou.Presence.Status != DSharpPlus.Entities.UserStatus.Offline)
                {
                    await ctx.RespondAsync("日本: " + timeInJapan +
                                           $"\n{userKou.Username} is up late (||or early?||) working on JPDB for us all <3")
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

        [Command("csttime")]
        [Cooldown(2, 10, CooldownBucketType.User)]
        [Description("Check CST time")]
        [Hidden]
        [Aliases("powtime", "powzukiatime", "lottetime", "jpdbsluttime", "peanutqueentime", "srsqueentime", "100%retentiontime")]
        public async Task csttime(CommandContext ctx)
        {
            Program.printCommandUse(ctx.User.Username, ctx.Message.Content);
            var info = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);
            String timeInJapan = localTime.ToString("yyyy/MM/dd HH:mm:ss");
            
            await ctx.RespondAsync("CET time: " + timeInJapan).ConfigureAwait(false);
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
            String flyntTime = localTime.ToString("MM/dd/yyyy HH:mm:ss");
            
            await ctx.RespondAsync($"EST: {flyntTime}").ConfigureAwait(false);
        }

        [Command("alemaxtime")]
        [Cooldown(2, 10, CooldownBucketType.User)]
        [Description("Check the time in Germany")]
        [Hidden]
        [Aliases("germanytime", "deutschtime", "germantime", "cesttime", "kaztime", "duditime", "frenchtime", "françaistime", "francaistime", "francetime", "cettime", "schnitzeltime", "alextime", "aletime", "n6time", "certifiedn6time")]
        public async Task alemaxtime(CommandContext ctx)
        {
            Program.printCommandUse(ctx.User.Username, ctx.Message.Content);
            var info = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);
            String alemaxTime = localTime.ToString("dd/MM/yyyy HH:mm:ss");

            await ctx.RespondAsync($"Europe Time: {alemaxTime}\n").ConfigureAwait(false);
        }

        [Command("uktime")]
        [Hidden]
        [Aliases("moemoekyuntime", "teatime", "moetime", "kingtime", "britishtime", "utc+1", "utc+1time", "jawtime", "jawgtime", "jawgboitime", "scottishtime", "scotstime", "queentime", "bri'ishtime")]
        [Cooldown(2, 10, CooldownBucketType.User)]
        [Description("Check the time in the UK")]
        public async Task jawgboitime(CommandContext ctx)
        {
            Program.printCommandUse(ctx.User.Username, ctx.Message.Content);
            var info = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);
            String jawgboiTime = localTime.ToString("yyyy/MM/dd HH:mm:ss");

            await ctx.RespondAsync($"GMT: {jawgboiTime}").ConfigureAwait(false);



            //var text = "Test 1\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n1.\n2." + "Test 2\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n.\n2.\n3.";

            //var interactivity = ctx.Client.GetInteractivity();
            //var embedPages = interactivity.GeneratePagesInEmbed(text);
            //var pages = interactivity.GeneratePagesInContent(text, SplitType.Character);

            //await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, embedPages);
        }


        [Command("indiantime")]
        [Cooldown(2, 10, CooldownBucketType.User)]
        [Description("Check the time in Japan")]
        [Hidden]
        [Aliases("indiatime", "indiastandardtime", "indianstandardtime", "100cardsadaytime")]
        public async Task kaztime(CommandContext ctx)
        {
            Program.printCommandUse(ctx.User.Username, ctx.Message.Content);
            var info = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);
            String timeInJapan = localTime.ToString("yyyy-MM-dd HH:mm:ss");
            await ctx.RespondAsync("India: " + timeInJapan).ConfigureAwait(false);
        }

        [Command("godtime")]
        [Cooldown(3, 20, CooldownBucketType.User)]
        [Hidden]
        [Aliases("kamisamatime", "神様time", "神さまtime")]
        public async Task godTime(CommandContext ctx)
        {
            try
            {
                if (ctx.User.Id == 399993082806009856) //alemax
                {
                    await ctx.Channel.TriggerTypingAsync();
                    System.Threading.Thread.Sleep(1000);
                    await ctx.RespondAsync($"God is everywhere... ||***ド イ ツ 人*** {DiscordEmoji.FromName(ctx.Client, ":schnitzel:")}||").ConfigureAwait(false);
                }
                else if (ctx.User.Id == 118408957416046593) //jaw: 630381088404930560   kou: 118408957416046593
                {
                    Program.printCommandUse(ctx.User.Username, ctx.Message.Content);
                    var info = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
                    DateTimeOffset localServerTime = DateTimeOffset.Now;
                    DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);
                    String timeInJapan = localTime.ToString("yyyy/MM/dd HH:mm:ss");

                    await ctx.Channel.TriggerTypingAsync();
                    System.Threading.Thread.Sleep(1000);
                    await ctx.RespondAsync($"Surely you know what time it is, Kou-sama UwU").ConfigureAwait(false);
                    System.Threading.Thread.Sleep(2000);
                    await ctx.Channel.TriggerTypingAsync();
                    System.Threading.Thread.Sleep(1000);
                    await ctx.Channel.SendMessageAsync($"Well I'll tell you anyway {DiscordEmoji.FromName(ctx.Client, ":kekmask:")}").ConfigureAwait(false);
                    System.Threading.Thread.Sleep(200);
                    await ctx.Channel.TriggerTypingAsync();
                    System.Threading.Thread.Sleep(500);
                    await ctx.Channel.SendMessageAsync($"It's {timeInJapan}").ConfigureAwait(false);

                }
                else if (ctx.User.Id == 377232289186447373) //pow
                {
                    await ctx.Channel.TriggerTypingAsync();
                    System.Threading.Thread.Sleep(1000);
                    await ctx.RespondAsync($"God is everywhere... ||***ア メ リ カ 人*** {DiscordEmoji.FromName(ctx.Client, ":0percentretentionrate:")}||").ConfigureAwait(false);
                }
                else if (ctx.User.Id == 245371520174522368) //flynt
                {
                    await ctx.Channel.TriggerTypingAsync();
                    System.Threading.Thread.Sleep(2000);
                    await ctx.RespondAsync($"God is everywhere... ||Haha you thought I was going to come up with some sort of special message {DiscordEmoji.FromName(ctx.Client, ":kuk:")}||").ConfigureAwait(false);
                    System.Threading.Thread.Sleep(2000);
                    await ctx.Channel.TriggerTypingAsync();
                    System.Threading.Thread.Sleep(500);
                    await ctx.RespondAsync($"Wait a minute...").ConfigureAwait(false);
                }
                else if (ctx.User.Id == 440138969645056010) //borre
                {
                    await ctx.Channel.TriggerTypingAsync();
                    System.Threading.Thread.Sleep(1000);
                    await ctx.RespondAsync($"God is everywhere... ||***ポ リ グ ロ ッ ト*** {DiscordEmoji.FromName(ctx.Client, ":ninja:")}||").ConfigureAwait(false);
                }
                else if (ctx.User.Id == 162762502336151554) //bijak
                {
                    await ctx.Channel.TriggerTypingAsync();
                    System.Threading.Thread.Sleep(1000);
                    await ctx.RespondAsync($"God is everywhere... ||***P E R S O N  I  C O U L D N ' T  C O M E  U P  W I T H  S O M E T H I N G  F U N N Y  F O R*** {DiscordEmoji.FromName(ctx.Client, ":kekmask:")}||").ConfigureAwait(false);
                }
                else if (ctx.User.Id == 173221089810317312) //neverside
                {
                    await ctx.Channel.TriggerTypingAsync();
                    System.Threading.Thread.Sleep(1000);
                    await ctx.RespondAsync($"God is everywhere... ||***天 気 用 法 人*** {DiscordEmoji.FromName(ctx.Client, ":sunrise_over_mountains:")}||").ConfigureAwait(false);
                }
                else if (ctx.User.Id == 435805456116482059) //kitsune
                {
                    await ctx.Channel.TriggerTypingAsync();
                    System.Threading.Thread.Sleep(1300);
                    await ctx.RespondAsync($"God is everywhere... ||or is he? ...||").ConfigureAwait(false);
                    System.Threading.Thread.Sleep(400);
                    await ctx.Channel.TriggerTypingAsync();
                    System.Threading.Thread.Sleep(900);
                    await ctx.RespondAsync($"What even is ~~existance~~ existence??...").ConfigureAwait(false);
                    System.Threading.Thread.Sleep(100);
                    await ctx.Channel.TriggerTypingAsync();
                    System.Threading.Thread.Sleep(500);
                    await ctx.RespondAsync($"So many questions... {DiscordEmoji.FromName(ctx.Client, ":germanface:")}").ConfigureAwait(false);
                }
                else if (ctx.User.Id == 131832027082260480) //sascha
                {
                    await ctx.Channel.TriggerTypingAsync();
                    System.Threading.Thread.Sleep(1300);
                    await ctx.RespondAsync($"God is everywhere... ||***ド イ ツ 人***||").ConfigureAwait(false);
                }
                else if (ctx.User.Id == 628550850113044501) //kaz
                {
                    await ctx.Channel.TriggerTypingAsync();
                    System.Threading.Thread.Sleep(1300);
                    await ctx.RespondAsync($"God is everywhere... ||***料 理 が 上  手 人*** {DiscordEmoji.FromName(ctx.Client, ":fork_knife_plate:")}||").ConfigureAwait(false);
                }
                else if (ctx.User.Id == 155192669520265216) //v
                {
                    System.Threading.Thread.Sleep(8000);
                    await ctx.Channel.TriggerTypingAsync();
                    System.Threading.Thread.Sleep(500);
                    await ctx.RespondAsync($"God is everywhere... ||haha you thought I wasn't going to respond {DiscordEmoji.FromName(ctx.Client, ":kuk:")}||").ConfigureAwait(false);
                }
                else if (ctx.User.Id == 265381159561723905) //酔いどれ戦士
                {
                    await ctx.Channel.TriggerTypingAsync();
                    System.Threading.Thread.Sleep(1000);
                    await ctx.RespondAsync($"God is everywhere... ||***酔 っ ぱ ら い*** {DiscordEmoji.FromName(ctx.Client, ":kuk:")}||").ConfigureAwait(false);
                }
                else
                {
                    await ctx.Channel.TriggerTypingAsync();
                    System.Threading.Thread.Sleep(1000);
                    await ctx.RespondAsync($"God is everywhere... ||***M O R T A L*** {DiscordEmoji.FromName(ctx.Client, ":kuk:")}||").ConfigureAwait(false);
                }
            } catch
            {
                await ctx.Channel.TriggerTypingAsync();
                System.Threading.Thread.Sleep(1000);
                await ctx.RespondAsync($"God is everywhere... ||***M O R T A L*** {DiscordEmoji.FromName(ctx.Client, ":kuk:")}||").ConfigureAwait(false);
            }          
        }

        [Command("tadokutime")]
        [Cooldown(2, 10, CooldownBucketType.User)]
        [Description("Check the time that Tadoku uses for its leader")]
        public async Task tadokuTime(CommandContext ctx)
        {
            Program.printCommandUse(ctx.User.Username, ctx.Message.Content);
            var info = TimeZoneInfo.FindSystemTimeZoneById("UTC");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);
            String tadokuTime = localTime.ToString("yyyy/MM/dd HH:mm:ss");
            await ctx.RespondAsync("Tadoku time: " + tadokuTime).ConfigureAwait(false);
        }

    }
}