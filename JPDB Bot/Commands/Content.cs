using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace JPDB_Bot.Commands
{
    public class Content : BaseCommandModule
    {
        string statisticsPage = string.Empty;

        [Command("content")]
        [Cooldown(3, 10, CooldownBucketType.User)]
        [Description("Search for content in the JPDB database\nFor example: !content \"steins gate\"")]
        public async Task SearchContent(CommandContext ctx,
            [DescriptionAttribute("Name of the content you are searching")] [RemainingText]
            string searchString)
        {
            Program.PrintCommandUse(ctx.User.Username, ctx.Message.Content);
            //await ctx.Channel.SendMessageAsync("Searching for " + searchString + "...").ConfigureAwait(false);
            await ContentDetails(ctx, searchString);
            await ContentStats(ctx, searchString);
        }

        private async Task ContentDetails(CommandContext ctx,
            string searchString)
        {
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;

            // Remove quotes if the search string is quoted ("stuff" -> stuff)
            if (searchString is { Length: >= 2 } && searchString[0] == '"' && searchString[^1] == '"')
            {
                searchString = searchString.Substring(1, searchString.Length - 2);
            }

            string url = "https://jpdb.io/prebuilt_decks?q=" + searchString;
            string html = "";
            try
            {
                html = client.DownloadString(new Uri(url));
            }
            catch (Exception ex)
            {
                Program.PrintError(ex.Message);
                return;
            }

            int snipIndex = html.IndexOf("30rem;\">") + 8;

            if (snipIndex == 7)
            {
                await ctx.RespondAsync("No content found UwU").ConfigureAwait(false);
                return;
            }

            string wordTemp = html.Substring(snipIndex);
            string wordTemp2 = html;
            html = wordTemp;

            snipIndex = wordTemp.IndexOf("<");
            html = wordTemp.Substring(snipIndex);
            string contentName = wordTemp.Substring(0, snipIndex);

            snipIndex = html.IndexOf("margin-top: 0.5rem;\">") + 22;
            wordTemp = html.Substring(snipIndex);
            snipIndex = wordTemp.IndexOf("/") + 1;
            wordTemp = wordTemp.Substring(snipIndex);
            snipIndex = wordTemp.IndexOf("\"");
            wordTemp = "https://jpdb.io/" + wordTemp.Substring(0, snipIndex);

            //getting statistics page:
            snipIndex = wordTemp2.IndexOf("dropdown-content") + 17;
            wordTemp2 = wordTemp2.Substring(snipIndex);
            snipIndex = wordTemp2.IndexOf("/");
            wordTemp2 = wordTemp2.Substring(snipIndex);
            snipIndex = wordTemp2.IndexOf("\"");
            wordTemp2 = wordTemp2.Substring(0, snipIndex);

            statisticsPage= "https://jpdb.io" + wordTemp2;


            await ctx.RespondAsync("Found " + contentName + ":\n" + wordTemp).ConfigureAwait(false);
        }

        private async Task ContentStats(CommandContext ctx,
            string searchString)
        {
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;

            // Remove quotes if the search string is quoted ("stuff" -> stuff)
            if (searchString is { Length: >= 2 } && searchString[0] == '"' && searchString[^1] == '"')
            {
                searchString = searchString.Substring(1, searchString.Length - 2);
            }

            string url = "https://jpdb.io/prebuilt_decks?q=" + searchString;
            string html = "";
            try
            {
                html = client.DownloadString(new Uri(url));
            }
            catch (Exception ex)
            {
                Program.PrintError(ex.Message);
                return;
            }



        }
    }
}