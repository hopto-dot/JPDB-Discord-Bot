using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

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
    }
}