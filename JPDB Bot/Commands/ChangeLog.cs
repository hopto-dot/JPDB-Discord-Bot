using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;

namespace JPDB_Bot.Commands
{
    public class ChangeLog : BaseCommandModule
    {
        [Command("changelog")]
        [Cooldown(1, 20, CooldownBucketType.User)]
        [Description("Shows the latest addition to the changelog")]
        public async Task changeLog(CommandContext ctx)
        {
            Program.PrintCommandUse(ctx.User.Username, ctx.Message.Content);
            string Date = String.Empty;

            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load("https://jpdb.io/changelog");
            string ChangeDate = htmlDoc.DocumentNode.SelectNodes("/html/body/div[2]/h5[1]")
                .First()
                .InnerHtml;
            string Information = htmlDoc.DocumentNode.SelectNodes("/html/body/div[2]/h5[1]").First().NextSibling
                .InnerHtml;

            await ctx.RespondAsync(ChangeDate).ConfigureAwait(false);
        }


        [Command("ping")]
        [Cooldown(2, 5, CooldownBucketType.User)]
        [Description("Pings a URL of your choice")]
        [Hidden]
        public async Task ping(CommandContext ctx, string URL, string showHTML = "false")
        {
            Program.PrintCommandUse(ctx.Message.Author.Username, ctx.Message.Content);
            string html = string.Empty;
            if (URL.Contains("https://") == false) { URL = "https://" + URL; }
            try
            {
                html = new WebClient { }.DownloadString(new Uri(URL));
                await ctx.RespondAsync($"Got a html response of length {html.Length}").ConfigureAwait(false);
                if ((showHTML.ToLower() == "true" || showHTML.ToLower() == "yes") && ctx.Message.Author.Username == "JawGBoi") { Program.PrintAPIUse(html, URL); }
            } catch
            {
                await ctx.RespondAsync($"Didn't get a response").ConfigureAwait(false);
            }
        }
    }

}