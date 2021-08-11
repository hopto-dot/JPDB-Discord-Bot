using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Entities;

namespace DiscordBot.Commands
{
    public class TestCommands : BaseCommandModule
    {
        [Command("hi")]
        [Cooldown(2, 10, CooldownBucketType.User)]
        [Description("Get a nice (or bad) response")]
        public async Task hi(CommandContext ctx)
        {
            Program.PrintCommandUse(ctx.User.Username, ctx.Message.Content);
            Random random = new Random();
            int randomInt = random.Next(1, 16);
            string Output = string.Empty;
            if (ctx.Member.Roles.Any(r => r.Name == "Owner" || r.Name == "Supporter" || r.Name == "Server Booster"))
            {
                switch (randomInt)
                {
                    case 1:
                        goto case 7;
                    case 2:
                        goto case 7;
                    case 3:
                        goto case 7;
                    case 4:
                        goto case 7;
                    case 5:
                        Output = "Hi " + ctx.User.Username + "様, I can speak English too ya know >:)";
                        break;
                    case 6:
                        goto case 11;
                    case 7:
                        Output = "どうも、" + ctx.User.Username + "様 :)";
                        break;
                    case 8:
                        Output = "どうも、" + ctx.User.Username + "様 ;)";
                        break;
                    case 9:
                        Output = "よおおおおおおおぉ " + ctx.User.Username + "様！ :)";
                        break;
                    case 10:
                        goto case 11;
                    case 11:
                        Output = "また会えて嬉しいね " + ctx.User.Username + "様 :)";
                        break;
                    case 12:
                        Output = "やっほおおおおおお～ " + ctx.User.Username + "様 :)";
                        break;
                    case 13:
                        Output = "おおおおっす! " + ctx.User.Username + "様 :)";
                        break;
                    case 14:
                        Output = "ハロオオオ " + ctx.User.Username + "様！ :)";
                        break;
                    case 15:
                        Output = "へっ！なんかあった？" + ctx.User.Username + "様 :O";
                        break;
                }
            }
            else
            {
                switch (randomInt)
                {
                    case 1:
                        goto case 7;
                    case 2:
                        goto case 7;
                    case 3:
                        goto case 7;
                    case 4:
                        goto case 7;
                    case 5:
                        goto case 7;
                    case 6:
                        goto case 11;
                    case 7:
                        Output = "どうも、" + ctx.User.Username + " :)";
                        break;
                    case 8:
                        Output = "どうも、" + ctx.User.Username + " ;)";
                        break;
                    case 9:
                        Output = "おいお前 JPDBの支援者になれ";
                        break;
                    case 10:
                        goto case 12;
                    case 11:
                        goto case 12;
                    case 12:
                        Output = "元気はないんだなあ " + ctx.User.Username + "さん。JPDBを支援したら？うwう";
                        break;
                    case 13:
                        Output = "おっす! " + ctx.User.Username + " :)";
                        break;
                    case 14:
                        Output = "ハロー " + ctx.User.Username + "！ :)";
                        break;
                    case 15:
                        Output = "へっ！なんかあった？" + ctx.User.Username + " :O";
                        break;
                }
            }
            await ctx.RespondAsync(Output).ConfigureAwait(false);
        }

        [Command("guessgame")]
        [Cooldown(1, 20, CooldownBucketType.User)]
        [Description("Play a word frequency guessing game against someone else")]
        public async Task guessgame(CommandContext ctx, [DescriptionAttribute("Your JPDB username")] string player1) //[DescriptionAttribute("Your JPDB username")] string player1
        {
            Program.PrintCommandUse(ctx.User.Username, ctx.Message.Content);

            DiscordUser Player1 = ctx.Message.Author;
            string User1 = player1;

            await ctx.Channel.SendMessageAsync($"Type \"!participate [username]\" to play with {ctx.User.Username}").ConfigureAwait(false);

            DSharpPlus.Interactivity.InteractivityResult<DSharpPlus.Entities.DiscordMessage> result;
            do {
                result = await ctx.Channel.GetNextMessageAsync(m =>
                {
                    return (m.Content.ToLower()).Substring(0, 13) == "!participate ";
                }).ConfigureAwait(false);
                if (result.TimedOut)
                {
                    await ctx.Channel.SendMessageAsync("Game timed out").ConfigureAwait(false);
                } 
                else if (result.Result.Author.Username == ctx.Message.Author.Username)
                {
                    //await ctx.RespondAsync("You can't play against yourself lol").ConfigureAwait(false);
                }
                
            } while (result.Result.Author.Username == ctx.Message.Author.Username && true == false);

            DiscordUser Player2 = result.Result.Author;
            string User2 = result.Result.Content.Substring(13);
            //await ctx.Channel.SendMessageAsync(User2);
            //await ctx.RespondAsync($"{Player1.Username} ({User1}) 対 {Player2.Username} ({User2})").ConfigureAwait(false);
            var gameEmbed = new DiscordEmbedBuilder
            {
                Title = "Guessing game",
                Description = $"{Player1.Username} ({User1}) 対 {Player2.Username} ({User2})",
                Color = DiscordColor.Red,
                Footer = new DiscordEmbedBuilder.EmbedFooter {
                    Text = "Who will win? :o",
                }
            };
            await ctx.Channel.SendMessageAsync(embed: gameEmbed).ConfigureAwait(false);
        }

        [Command("changelog")]
        [Cooldown(1, 20, CooldownBucketType.User)]
        [Description("Shows the latest addition to the changelog")]
        public async Task changeLog(CommandContext ctx)
        {
            Program.PrintCommandUse(ctx.User.Username, ctx.Message.Content);
            string Date = String.Empty;

            /*WebClient Client = new WebClient();
            Client.Encoding = System.Text.Encoding.UTF8;
            string HTML = "";
            string sniptemp = string.Empty;
            HTML = Client.DownloadString(new Uri("https://jpdb.io/changelog"));
            int snipIndex = HTML.IndexOf("<h5 id=") + 8;
            HTML = HTML.Substring(snipIndex);
            snipIndex = HTML.IndexOf(">") + 1;
            HTML = HTML.Substring(snipIndex);

            sniptemp = HTML;
            snipIndex = sniptemp.IndexOf("<");
            sniptemp = sniptemp.Substring(0, snipIndex);

            var changelogEmbed = new discordembed*/

            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load("https://jpdb.io/changelog");
            string ChangeDate = htmlDoc.DocumentNode.SelectNodes("/html/body/div[2]/h5[1]")
    .First()
    .InnerHtml;
            string Information = htmlDoc.DocumentNode.SelectNodes("/html/body/div[2]/h5[1]").First().NextSibling.InnerHtml;

            await ctx.RespondAsync(ChangeDate).ConfigureAwait(false);
        }

        [Command("content")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [Description("Search for content in the JPDB database\nFor example: !content \"steins gate\"")]
        public async Task content(CommandContext ctx, [DescriptionAttribute("Name of the content you are searching")] string searchString)


        {
            Program.PrintCommandUse(ctx.User.Username, ctx.Message.Content);
            //await ctx.Channel.SendMessageAsync("Searching for " + searchString + "...").ConfigureAwait(false);


            WebClient Client = new WebClient();
            Client.Encoding = System.Text.Encoding.UTF8;
            string HTML = "";
            int snipIndex = -1;

            string OriginalFilter = "anime";
            bool ScrapeKanji = false;
            

            if (HTML.Length > 250) return;

            int snipIndex2 = -1;

            bool pageDone = false;
            int pageInterval = 0;
            string wordTemp = "";
            string URL = "";
            List<string> wordIDs = new List<string>() { };
            URL = "https://jpdb.io/prebuilt_decks?q=" + searchString;
            try
            {
                HTML = Client.DownloadString(new Uri(URL));
            }
            catch (Exception ex)
            {
                pageDone = true;
            }

            snipIndex = HTML.IndexOf("30rem;\">") + 8;

            if (snipIndex == 7)
            {
                await ctx.RespondAsync("No content found UwU").ConfigureAwait(false);
                return;
            }
            wordTemp = HTML.Substring(snipIndex);
            HTML = wordTemp;

            snipIndex = wordTemp.IndexOf("<");
            HTML = wordTemp.Substring(snipIndex);
            string contentName = wordTemp.Substring(0, snipIndex);

            snipIndex = HTML.IndexOf("margin-top: 0.5rem;\">") + 22;
            wordTemp = HTML.Substring(snipIndex);
            snipIndex = wordTemp.IndexOf("/") + 1;
            wordTemp = wordTemp.Substring(snipIndex);
            snipIndex = wordTemp.IndexOf("\"");
            wordTemp = "https://jpdb.io/" + wordTemp.Substring(0, snipIndex);

            await ctx.RespondAsync("Found " + contentName + ":\n" + wordTemp).ConfigureAwait(false);

            int Frequency = 1;
            if (wordTemp.Contains(">") == false && wordTemp.Contains("<") == false & wordTemp.Contains("=") == false & wordTemp.Contains("-") == false)
                {
            try
                {
                for (var I = 1; I <= Frequency; I++)
                wordIDs.Add(wordTemp);
                }
            catch (Exception ex)
                {
                }
            }

                    snipIndex = HTML.IndexOf("#a") + 3;
        }

        [Command("japantime")]
        [Cooldown(2, 10, CooldownBucketType.User)]
        [Description("Check the time in Japan")]
        public async Task japantime(CommandContext ctx)
        {
            Program.PrintCommandUse(ctx.User.Username, ctx.Message.Content);
            var info = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);
            String TimeInJapan = localTime.ToString("dd/MM/yyyy HH:mm:ss");
            var Kou = await ctx.Client.GetUserAsync(118408957416046593);
            if ((localTime.Hour > 21 || localTime.Hour < 5) && Kou.Presence.Status != DSharpPlus.Entities.UserStatus.Offline)
            {
                await ctx.RespondAsync("日本: " + TimeInJapan + $"\n{Kou.Username} is up last working on JPDB for us all <3").ConfigureAwait(false);
            } else
            {
                await ctx.RespondAsync("日本: " + TimeInJapan).ConfigureAwait(false);
            }
        }
    }


}
